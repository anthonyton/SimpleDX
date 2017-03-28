using SharpDX.DXGI;
using SharpDX.Windows;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace SharpDX.Framework
{
    public abstract class D3DApplicationDesktop : D3DApplicationBase
    {
        Form window;

        public Form Window { get { return window; } }

        public D3DApplicationDesktop(Form window)
        {
            this.window = window;
            Window.SizeChanged += Window_SizeChanged;
        }

        private void Window_SizeChanged(object sender, EventArgs e)
        {
            SizeChanged();
        }

        public override Rectangle CurrentBounds
        {
            get
            {
                return new Rectangle(window.ClientRectangle.X, window.ClientRectangle.Y, window.ClientRectangle.Width, window.ClientRectangle.Height);
            }
        }

        protected virtual SwapChainFullScreenDescription CreateFullScreenDescription()
        {
            return new SwapChainFullScreenDescription()
            {
                RefreshRate = new Rational(60, 1),
                Scaling = DisplayModeScaling.Centered,
                Windowed = true
            };
        }

        protected override SwapChain1 CreateSwapChain(Factory2 factory, SharpDX.Direct3D11.Device1 device, SwapChainDescription1 desc)
        {
            return new SwapChain1(factory, device, Window.Handle, ref desc, CreateFullScreenDescription(), null);
        }
    }
}
