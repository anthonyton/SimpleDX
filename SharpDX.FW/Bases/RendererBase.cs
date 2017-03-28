using SharpDX;
using SharpDX.Mathematics.Interop;
using D3D11 = SharpDX.Direct3D11;

namespace SharpDX.Framework
{
    public abstract class RendererBase : Component
    {
        public DeviceManager DeviceManager { get; protected set; }
        public D3DApplicationBase Target { get; protected set; }
        public virtual bool Show { get; set; }
        public Matrix3x2 World;

        D3D11.DeviceContext rendererContext = null;
        /// <summary>
        /// Umožní specifikovat kontext pro renderování
        /// </summary>
        public D3D11.DeviceContext RenderContext
        {
            get { return rendererContext ?? this.DeviceManager.Direct3DContext; }
            set { rendererContext = value; }
        }

        public RendererBase()
        {
            World = Matrix3x2.Identity;
            Show = true;
        }

        /// <summary>
        /// Inicializuje renderer s konkrétním DeviceManagerem, který je poskytnut danou třídou aplikace
        /// </summary>
        /// <param name="app"></param>
        public virtual void Initialize(D3DApplicationBase app)
        {
            //Jestliže chceme reinicializovat, je zde pravděpodobně devicemanager, proto odstraníme handler
            if (this.DeviceManager != null)
                this.DeviceManager.OnInitialize -= DeviceManager_OnInitialize;

            this.DeviceManager = app.DeviceManager;

            //Nastavíme novému deviceManageru handler
            this.DeviceManager.OnInitialize += DeviceManager_OnInitialize;

            //Stejné jako u reinicializace devicemanageru
            if (this.Target != null)
                this.Target.OnSizeChanged -= Target_OnSizeChanged;

            this.Target = app;

            this.Target.OnSizeChanged += Target_OnSizeChanged;

            //jestliže je zařízení v deviceManageru již inicializováno, vytvoříme prostředky
            if (this.DeviceManager.Direct3DDevice != null)
                CreateDeviceDependentResources();
           
        }

        void DeviceManager_OnInitialize(DeviceManager deviceManager)
        {
            CreateDeviceDependentResources();
        }

        void Target_OnSizeChanged(D3DApplicationBase target)
        {
            CreateSizeDependentResources();
        }

        /// <summary>
        /// Vytvoří prostředky, které závisí na zařízení nebo kontextu zařízení
        /// </summary>
        protected virtual void CreateDeviceDependentResources() { }

        /// <summary>
        /// Vytvoří prostředky, které závisí na velikosti Render targetu
        /// </summary>
        protected virtual void CreateSizeDependentResources() { }

        
        /// <summary>
        /// Potomci třídy <see cref="RendererBase"/> mohou renderovat 
        /// </summary>
        protected abstract void DoRender();

        /// <summary>
        /// Vyrenderuje snímek
        /// </summary>
        protected virtual void DoRender(D3D11.DeviceContext context) { }

        public void Render()
        {
            if (Show)
                DoRender();
        }

        public void Render(D3D11.DeviceContext context)
        {
            if (Show)
                DoRender(context);
        }


    }
}
