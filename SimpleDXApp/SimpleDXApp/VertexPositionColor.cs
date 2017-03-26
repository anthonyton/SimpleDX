using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using System.Runtime.InteropServices;
namespace SimpleDXApp
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColor
    {
        public readonly Vector3 position;
        public readonly Color4 color;
        public VertexPositionColor(Vector3 position, Color4 color)
        {
            this.position = position;
            this.color = color;
        }
    }
}
