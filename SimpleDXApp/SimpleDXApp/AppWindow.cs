using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.D3DCompiler;

namespace SimpleDXApp
{
    public class AppWindow : IDisposable
    {
        private const int Width = 1440;
        private const int Height = 900;

        private RenderForm window;

        private D3D11.Device d3dDevice;
        private SwapChain swapChain;
        private D3D11.DeviceContext d3dDeviceContext;
        private D3D11.RenderTargetView backBuffer;

        private D3D11.PixelShader pixelShader;
        private D3D11.VertexShader vertexShader;
        private ShaderSignature inputSignature;
        private D3D11.InputLayout inputLayout;

        private D3D11.InputElement[] inputElements = new D3D11.InputElement[]
        {
            new D3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
            new D3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 0)
        };

        /*  public Vector3[] vertices = new Vector3[]
          {
              new Vector3(-0.5f, 0.5f, 0.0f),
              new Vector3(0.5f, 0.5f, 0.0f),
              new Vector3(0.0f, -0.5f, 0.0f)
          };*/

        public VertexPositionColor[] vertices = new VertexPositionColor[]
        {
            new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.0f), new Color4(0.0f, 0.0f, 1.0f, 0.0f)),
            new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.0f), new Color4(0.0f, 1.0f, 0.0f, 0.0f)),
            new VertexPositionColor(new Vector3(0.0f, -0.5f, 0.0f), new Color4(1.0f, 0.0f, 0.0f, 0.0f))

        };

        private D3D11.Buffer triangleVertexBuffer;

        public AppWindow()
        {
            window = new RenderForm("SimpleDXApp");
            window.ClientSize = new System.Drawing.Size(Width, Height);
            window.BackColor = System.Drawing.Color.Empty;

            InitD3D();
            InitializeShaders();
            InitializeTriangle();
        }

        private void InitD3D()
        {
            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                BufferCount = 1,
                Usage = Usage.RenderTargetOutput,
                SampleDescription = new SampleDescription(1, 0),
                ModeDescription = new ModeDescription(Width, Height, new Rational(60, 1), Format.B8G8R8A8_UNorm),
                Flags = SwapChainFlags.AllowModeSwitch,
                IsWindowed = true,
                OutputHandle = window.Handle
            };

            D3D11.Device.CreateWithSwapChain(DriverType.Hardware, D3D11.DeviceCreationFlags.None, swapChainDesc, out d3dDevice, out swapChain);
            d3dDeviceContext = d3dDevice.ImmediateContext;

            using (var renderTarget = swapChain.GetBackBuffer<D3D11.Texture2D>(0))
                backBuffer = new D3D11.RenderTargetView(d3dDevice, renderTarget);

            ViewportF viewport = new ViewportF() { X = 0, Y = 0, Width = Width, Height = Height };
            d3dDeviceContext.Rasterizer.SetViewport(viewport);
    
        }


        private void InitializeShaders()
        {
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("vertexShader.hlsl", "main", "vs_4_0", ShaderFlags.Debug))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShader = new D3D11.VertexShader(d3dDevice, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("pixelShader.hlsl", "main", "ps_4_0", ShaderFlags.Debug))
                pixelShader = new D3D11.PixelShader(d3dDevice, pixelShaderByteCode);

            d3dDeviceContext.VertexShader.Set(vertexShader);
            d3dDeviceContext.PixelShader.Set(pixelShader);
            d3dDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            inputLayout = new D3D11.InputLayout(d3dDevice, inputSignature, inputElements);
            d3dDeviceContext.InputAssembler.InputLayout = inputLayout;
        }

        private void InitializeTriangle()
        {
            triangleVertexBuffer = D3D11.Buffer.Create(d3dDevice, D3D11.BindFlags.VertexBuffer, vertices);
        }

        public void Run()
        {
            RenderLoop.Run(window, RenderCallBack);
        }

        private void RenderCallBack()
        {
            RenderFrame();
        }

        private void RenderFrame()
        {
            d3dDeviceContext.OutputMerger.SetRenderTargets(backBuffer);
            d3dDeviceContext.ClearRenderTargetView(backBuffer, Color.LightBlue);
            //   d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(triangleVertexBuffer, Utilities.SizeOf<Vector3>(), 0));
            d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(triangleVertexBuffer, Utilities.SizeOf<VertexPositionColor>(), 0));
            d3dDeviceContext.Draw(vertices.Length, 0);
            swapChain.Present(0, PresentFlags.None);
            InitializeTriangle();
        }



        public void Dispose()
        {
            d3dDevice.Dispose();
            swapChain.Dispose();
            d3dDeviceContext.Dispose();
            backBuffer.Dispose();

            pixelShader.Dispose();
            vertexShader.Dispose();
            inputLayout.Dispose();
            inputSignature.Dispose();

            triangleVertexBuffer.Dispose();
        }



    }
}
