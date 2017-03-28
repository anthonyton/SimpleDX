using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
namespace SharpDX.Framework
{
    public class Component : ComponentBase, IDisposable
    {
        protected DisposeCollector DisposeCollector { get; set; }

        protected internal Component() { }
        protected Component(string name) : base(name)
        { }

        internal bool IsAttached { get; set; }

        protected internal bool IsDisposed { get; private set; }

        protected internal bool IsDisposing { get; protected set; }


        public event EventHandler<EventArgs> Disposing;

        /// <summary>
        /// Uvolní všechny nespravované a volitelně spravované prostředky
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposing = true;
                Disposing(this, EventArgs.Empty);
            }

            Dispose(true);
            IsDisposed = true;
        }

        /// <summary>
        /// Uvolní prostředky objektu
        /// </summary>
        /// <param name="disposeManagedResources">Pokud je nastaveno na true, spravované prostředky by měly být uvolněné spolu s nespravovanými</param>
        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                if (DisposeCollector != null)
                    DisposeCollector.Dispose();
                DisposeCollector = null;
            }
        }

        /// <summary>
        /// Přidá disposable objekt do listu objektů k dispose
        /// </summary>
        /// <param name="toDisposeArg"></param>
        /// <returns></returns>
        protected internal T ToDispose<T>(T toDisposeArg)
        {
            if (!ReferenceEquals(toDisposeArg, null))
            {
                if (DisposeCollector == null)
                    DisposeCollector = new DisposeCollector();
                return DisposeCollector.Collect(toDisposeArg);
            }
            return default(T);
        }

        /// <summary>
        /// Uvolní prostředky objektu, nastaví referenci na null a odstraní ho z listu
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToDispose"></param>
        protected internal void RemoveAndDispose<T>(ref T objectToDispose)
        {
            if (!ReferenceEquals(objectToDispose, null) && DisposeCollector != null)
                DisposeCollector.RemoveAndDispose(ref objectToDispose);
        }
        
        protected internal void RemoveToDispose<T>(T toDisposeArg)
        {
            if (!ReferenceEquals(toDisposeArg, null) && DisposeCollector != null)
                DisposeCollector.Remove(toDisposeArg);
        }

    }
}
