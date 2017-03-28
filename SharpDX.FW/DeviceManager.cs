
using SharpDX.Direct3D;
using SharpDX;
using D3D11 = SharpDX.Direct3D11;
using D2D1 = SharpDX.Direct2D1;
using System;

namespace SharpDX.Framework
{
    public class DeviceManager : Component
    {
        //Objekty Direct3D
        protected D3D11.Device1 d3dDevice;
        protected D3D11.DeviceContext1 d3dContext;
        protected float dpi;

        //Objekty Direct2D
        protected D2D1.Factory1 d2dFactory;
        protected D2D1.Device d2dDevice;
        protected D2D1.DeviceContext d2dContext;


        //Deklarace DirectWrite a Windows Imagining Component Objects
        protected SharpDX.DirectWrite.Factory dwriteFactory;
        protected SharpDX.WIC.ImagingFactory2 wicFactory;

        public FeatureLevel[] Direct3DFeaturesLevels = new FeatureLevel[]
        {
            FeatureLevel.Level_11_1,
            FeatureLevel.Level_11_0
            //FeatureLevel.Level_10_1,
            //FeatureLevel.Level_10_0
        };

        /// <summary>
        /// Vrátí Device DirectD311
        /// </summary>
        public D3D11.Device1 Direct3DDevice { get { return d3dDevice; } }

        /// <summary>
        /// Vrátí DeviceContext Direct3D11
        /// </summary>
        public D3D11.DeviceContext1 Direct3DContext { get { return d3dContext; } }


        /// <summary>
        /// Vrátí Factory Direct2D
        /// </summary>
        public D2D1.Factory1 Direct2DFactory { get { return d2dFactory; } }

        /// <summary>
        /// Vrátí Device Direct2D
        /// </summary>
        public D2D1.Device Direct2DDevice { get { return d2dDevice; } }

        /// <summary>
        /// Vrátí DeviceContext Direct2D
        /// </summary>
        public D2D1.DeviceContext Direct2DContext { get { return d2dContext; } }

        /// <summary>
        /// Vrátí DirectWrite factory
        /// </summary>
        public SharpDX.DirectWrite.Factory DirectWriteFactory { get { return dwriteFactory; } }


        /// <summary>
        /// Vrátí WIC factory
        /// </summary>
        public SharpDX.WIC.ImagingFactory2 WICFactory { get { return wicFactory; } }

        /// <summary>
        /// Vrátí nebo nastaví DPI
        /// </summary>
        public virtual float Dpi
        {
            get { return dpi; }
            set
            {
                if (dpi != value)
                {
                    dpi = value;
                    d2dContext.DotsPerInch = new Size2F(dpi, dpi);
                    if (OnDpiChanged != null)
                        OnDpiChanged(this);
                }
            }
        }

        /// <summary>
        /// Tato událost je vyvolána při inicializování DeviceManagera metodou <see cref="Initialize"/> 
        /// </summary>
        public event Action<DeviceManager> OnInitialize;

        /// <summary>
        /// Tato událost je vyvolána při změně property <see cref="Dpi"/> 
        /// </summary>
        public event Action<DeviceManager> OnDpiChanged;

        public virtual void Initialize(float dpi = 96.0f)
        {
            CreateInstances();

            if (OnInitialize != null)
                OnInitialize(this);

            Dpi = dpi;
        }

        protected virtual void CreateInstances()
        {
            //Nastaví reference prostředků na null a uvolní je
            RemoveAndDispose(ref d3dDevice);
            RemoveAndDispose(ref d3dContext);
            RemoveAndDispose(ref d2dDevice);
            RemoveAndDispose(ref d2dContext);
            RemoveAndDispose(ref d2dFactory);
            RemoveAndDispose(ref dwriteFactory);
            RemoveAndDispose(ref wicFactory);

            var creationFlags = D3D11.DeviceCreationFlags.BgraSupport | D3D11.DeviceCreationFlags.Debug;


            //Získá Direct3D 11.1 device a device kontext
            using (var device = new D3D11.Device(DriverType.Hardware, creationFlags, Direct3DFeaturesLevels))
                d3dDevice = ToDispose<D3D11.Device1>(device.QueryInterface<D3D11.Device1>());
            //Získá device context D3D11.1
            d3dContext = ToDispose<D3D11.DeviceContext1>(d3dDevice.ImmediateContext.QueryInterface<D3D11.DeviceContext1>());

            //Alokuje nové reference
            d2dFactory = ToDispose<D2D1.Factory1>(new D2D1.Factory1(SharpDX.Direct2D1.FactoryType.SingleThreaded, SharpDX.Direct2D1.DebugLevel.None));
            dwriteFactory = ToDispose<SharpDX.DirectWrite.Factory>(new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared));
            wicFactory = ToDispose<SharpDX.WIC.ImagingFactory2>(new SharpDX.WIC.ImagingFactory2());

            //Vytvoří D2D zařízení
            using (var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device>())
                d2dDevice = ToDispose<D2D1.Device>(new D2D1.Device(d2dFactory, dxgiDevice));

            //Vytvoří Direct2D context
            d2dContext = ToDispose<D2D1.DeviceContext>(new D2D1.DeviceContext(d2dDevice, D2D1.DeviceContextOptions.None));


        }


    }
}
