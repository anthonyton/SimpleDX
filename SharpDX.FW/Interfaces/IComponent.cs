using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX.Framework.Interfaces
{
    public interface IComponent
    {
        /// <summary>
        /// Vrátí nebo nastaví jméno komponenty
        /// </summary>
        string Name { get; set; }
    }
}
