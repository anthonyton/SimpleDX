using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using D2D1 = SharpDX.Direct2D1;
using Device1 = SharpDX.Direct3D11.Device1;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.Mathematics.Interop;

using SharpDX.Framework;
namespace SharpDX.Framework
{
    public abstract class D3DApplicationBase : Component
    {
        protected DeviceManager deviceManager;
        protected SwapChain1 swapChain;

        protected D3D11.RenderTargetView renderTargetView;
        protected D3D11.DepthStencilView depthStencilView;
        protected D3D11.Texture2D backBuffer;
        protected D3D11.Texture2D depthBuffer;

        protected D2D1.Bitmap1 bitmapTarget;


        public Rectangle Bounds { get; protected set; }

        public abstract Rectangle CurrentBounds { get; }

        public DeviceManager DeviceManager { get { return deviceManager; } }

        public SwapChain1 SwapChain
        {
            get { return swapChain; }
        }

        /// <summary>
        /// Poskytuje přístup to listu dostupných režimů zobrazování
        /// </summary>
        public ModeDescription[] DisplayModeList { get; private set; }

        /// <summary>
        /// Vrátí nebo nastaví zda swap-chain bude čekat na další vertikální synchronizaci před vykreslením
        /// </summary>
        public bool VSync { get; set; }

        /// <summary>
        /// Vrátí šířku swap-chain bufferu
        /// </summary>
        public virtual int Width
        {
            get
            {
                return (int)(Bounds.Width * DeviceManager.Dpi / 96.0);
            }
        }

        /// <summary>
        /// Vrátí výšku swap-chain bufferu
        /// </summary>
        public virtual int Height
        {
            get
            {
                return (int)(Bounds.Height * DeviceManager.Dpi / 96.0);
            }
        }

        /// <summary>
        /// Nastaví nebo vrátí ViewPort, který mapuje normalizované souřadnice rohů na pixely
        /// </summary>
        protected ViewportF ViewPort { get; set; }

        /// <summary>
        /// Vrátí nebo nastaví Rectangle závislý na RenderTargetu
        /// </summary>
        protected Rectangle RenderTargetBounds { get; set; }

        /// <summary>
        /// Vrátí rozměry RenderTargetu
        /// </summary>
        protected Size2 RenderTargetSize { get { return new Size2(RenderTargetBounds.Width, RenderTargetBounds.Height); } }


        public D3D11.RenderTargetView RenderTargetView
        {
            get { return renderTargetView; }
            protected set
            {
                if (renderTargetView != value)
                {
                    RemoveAndDispose(ref renderTargetView);
                    renderTargetView = value;
                }
            }
        }
        /// <summary>
        /// Vrátí nebo nastaví backbuffer
        /// </summary>
        public D3D11.Texture2D BackBuffer
        {
            get { return backBuffer; }
            protected set
            {
                if (backBuffer != value)
                {
                    RemoveAndDispose(ref backBuffer);
                    backBuffer = value;
                }
            }
        }

        /// <summary>
        /// Vrátí nebo nastaví depthBuffer
        /// </summary>
        public D3D11.Texture2D DepthBuffer
        {
            get { return depthBuffer; }
            protected set
            {
                if (depthBuffer != value)
                {
                    RemoveAndDispose(ref depthBuffer);
                    depthBuffer = value;
                }
            }
        }

        /// <summary>
        /// Vrátí nebo nastaví D3D DepthStencilView
        /// </summary>
        public D3D11.DepthStencilView DepthStencilView
        {
            get { return depthStencilView; }
            set
            {
                if (depthStencilView != value)
                {
                    RemoveAndDispose(ref depthStencilView);
                    depthStencilView = value;
                }
            }
        }

        /// <summary>
        /// Vrátí nebo nastaví D2D RenderTargetView
        /// </summary>
        public D2D1.Bitmap1 BitmapTarget2D
        {
            get { return bitmapTarget; }
            protected set
            {
                if (bitmapTarget != value)
                {
                    RemoveAndDispose(ref bitmapTarget);
                    bitmapTarget = value;
                }
            }
        }

        public D3DApplicationBase()
        {
            deviceManager = ToDispose(new DeviceManager());
            DeviceManager.OnInitialize += CreateDeviceDependentResources;
            this.OnSizeChanged += CreateSizeDependentResources;
        }
        
        /// <summary>
        /// Inicializuje DeviceManagera a vyvolá událost <see cref="SizeChanged(bool)"/> 
        /// </summary>
        public virtual void Initialize()
        {
            DeviceManager.Initialize();

            SizeChanged();
        }


        protected void SizeChanged(bool force = false)
        {
            var newbounds = CurrentBounds;
            if ((newbounds.Width == 0 && newbounds.Height != 0) ||
                 newbounds.Width != 0 && newbounds.Height == 0)
                return;
            if (newbounds.Width != Bounds.Width || newbounds.Height != Bounds.Height || force)
            {
                Bounds = newbounds;
                if (OnSizeChanged != null)
                    OnSizeChanged(this);
            }
        }


        /// <summary>
        /// Vytvoří prostředky závislé na Device/> 
        /// </summary>
        /// <param name="deviceManager"></param>
        protected virtual void CreateDeviceDependentResources(DeviceManager deviceManager)
        {
            if (swapChain != null)
            {
                //Uvolní prostředky SwapChainu
                RemoveAndDispose(ref swapChain);
                SizeChanged(true);
            }
        }

        /// <summary>
        /// Vytvoří prostředky závislé na rozměrech výstupu
        /// </summary>
        /// <param name="app"></param>
        protected virtual void CreateSizeDependentResources(D3DApplicationBase app)
        {
            //Získáme reference na device a kontext
            var device = DeviceManager.Direct3DDevice;
            var context = DeviceManager.Direct3DContext;

            //Získáme D2D kontext (na renderování textu apod.)
            var d2dContext = DeviceManager.Direct2DContext;
           
            //Předtím než swapchain může změnit velikost bufferů, všechny buffery musí být uvolněny
            RemoveAndDispose(ref backBuffer);
            RemoveAndDispose(ref renderTargetView);
            RemoveAndDispose(ref depthStencilView);
            RemoveAndDispose(ref depthBuffer);
            RemoveAndDispose(ref bitmapTarget);
            d2dContext.Target = null;
      
            #region Inicializace Direct3D swapChainu a RenderTargetu
            //Jestliže swapchain již existuje, změn velikost
            if (swapChain != null)
            {
                swapChain.ResizeBuffers(swapChain.Description1.BufferCount,
                                        Width,
                                        Height,
                                        swapChain.Description1.Format,
                                        swapChain.Description1.Flags);
            }
            //Jinak vytvoř nový
            else
            {
                var desc = CreateSwapChainDescription();

                using (var dxgiDevice2 = device.QueryInterface<Device2>())
                using (var dxgiAdapter = dxgiDevice2.Adapter)
                using (var dxgiFactory2 = dxgiAdapter.GetParent<Factory2>())
                using (var output = dxgiAdapter.Outputs.First())
                {
                    swapChain = ToDispose(CreateSwapChain(dxgiFactory2, device, desc));

                    //Získá list podporovaných zobrazovacích režimů
                    DisplayModeList = output.GetDisplayModeList(desc.Format, DisplayModeEnumerationFlags.Scaling);
                }
            }

            //Získá backbuffer pro toto okno, do kterého se bude renderovat
            BackBuffer = ToDispose(D3D11.Texture2D.FromSwapChain<D3D11.Texture2D>(swapChain, 0));
            //Vytvoří pohled pro přístup k prostředkům
            RenderTargetView = ToDispose(new D3D11.RenderTargetView(device, backBuffer));

            var backBufferDesc = BackBuffer.Description;
            RenderTargetBounds = new Rectangle(0, 0, backBufferDesc.Width, backBufferDesc.Height);

            //Vytvoříme viewPort, který mapuje normalizované souřadnice rohů okna na pixely
            this.ViewPort = new ViewportF(
                (float)RenderTargetBounds.X,
                (float)RenderTargetBounds.Y,
                (float)RenderTargetBounds.Width,
                (float)RenderTargetBounds.Height,
                0.0f,
                1.0f);

            context.Rasterizer.SetViewport(this.ViewPort);


            this.DepthBuffer = ToDispose(new D3D11.Texture2D(device, new D3D11.Texture2DDescription()
            {
                Format = SharpDX.DXGI.Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = RenderTargetSize.Width,
                Height = RenderTargetSize.Height,
                SampleDescription = SwapChain.Description.SampleDescription,
                BindFlags = D3D11.BindFlags.DepthStencil,
            }));

            this.DepthStencilView = ToDispose(
                new D3D11.DepthStencilView(
                    device,
                    DepthBuffer,
                    new D3D11.DepthStencilViewDescription()
                    {
                        Dimension = (SwapChain.Description.SampleDescription.Count > 1 || SwapChain.Description.SampleDescription.Quality > 0) ? D3D11.DepthStencilViewDimension.Texture2DMultisampled : D3D11.DepthStencilViewDimension.Texture2D
                    }));
            context.OutputMerger.SetTargets(DepthStencilView, RenderTargetView);


            #endregion

            #region Inicializace D2D render targetu
            //Zde se bude nastavovat Direct2D render target, který bude napojen na swapchain
            //Kdykoliv budeme renderovat do této bitmapy, bude se to přímo renderovat do swapchainu

            var bitmapProperties = new D2D1.BitmapProperties1(
                new D2D1.PixelFormat(swapChain.Description.ModeDescription.Format, D2D1.AlphaMode.Premultiplied),
                DeviceManager.Dpi,
                DeviceManager.Dpi,
                D2D1.BitmapOptions.Target | D2D1.BitmapOptions.CannotDraw);

            //Direct2D potřebuje DXGI verzi backbuffer surface pointeru
            //Tato D2D plocha z DXGI backbufferu bude použita jako render target
            using (var dxgiBackBuffer = SwapChain.GetBackBuffer<Surface>(0))
                BitmapTarget2D = ToDispose(new D2D1.Bitmap1(d2dContext, dxgiBackBuffer, bitmapProperties));

            d2dContext.Target = BitmapTarget2D;

            d2dContext.TextAntialiasMode = D2D1.TextAntialiasMode.Grayscale;
         
            #endregion
        }



        /// <summary>
        /// Vytvoří swapchain description.
        /// </summary>
        /// <returns></returns>
        protected virtual SwapChainDescription1 CreateSwapChainDescription()
        {
            var desc = new SwapChainDescription1()
            {
                Width = Width,
                Height = Height,
                Format = Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.BackBuffer | Usage.RenderTargetOutput,
                BufferCount = 1,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.Discard,
                Flags = SwapChainFlags.AllowModeSwitch
            };
            return desc;
        }

        /// <summary>
        /// Vytvoří swapchain
        /// </summary>
        /// <param name="factory">DXGI Factory</param>
        /// <param name="device">D3D11 Device</param>
        /// <param name="desc">SwapChainDescription</param>
        /// <returns></returns>
        protected abstract SwapChain1 CreateSwapChain(Factory2 factory, D3D11.Device1 device, SwapChainDescription1 desc);

        public abstract void Run();

        /// <summary>
        /// Vyrenderuje backbuffer swapchainu
        /// </summary>
        public virtual void Present()
        {
            var parameters = new PresentParameters();
            try
            {
                //Pokud je aktivovaná vertikální synchronizace, DXGI bude blokovat do VSync
                //uspí aplikaci do spuštění další VSync
                //Toto zajistí, že nebudeme plýtvat CPU/GPU cykly renderováním snímků, které se nikdy nezobrazí na obrazovce
                swapChain.Present((VSync ? 1 : 0), PresentFlags.None, parameters);
            }
            catch (SharpDXException ex)
            {
                //pokud zobrazovací zařízení bylo vypojeno nebo resetováno, musíme reinicializovat renderer.
                if (ex.ResultCode == ResultCode.DeviceRemoved
                    || ex.ResultCode == ResultCode.DeviceReset)
                    DeviceManager.Initialize(DeviceManager.Dpi);
                else
                    throw;
            }
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
                if (SwapChain != null)
                {
                    //Musíme se ujistit, že nejsme ve fullscrenu, jinak zařízení bude vyhazovat výjimku
                    SwapChain.IsFullScreen = false;
                }
            base.Dispose(disposeManagedResources);
        }
        /// <summary>
        /// Vyvolá událost při změně renderovacího okna
        /// </summary>
        public event Action<D3DApplicationBase> OnSizeChanged;

    }
}
