using System;
using System.ComponentModel;

namespace SharpDX.Framework
{
    public abstract class ComponentBase : Interfaces.IComponent, INotifyPropertyChanged
    {
        private string name;

        private readonly bool isNameImmutable;

        private object tag;

        protected ComponentBase() { }
        protected ComponentBase(string name)
        {
            if (name != null)
            {
                this.name = name;
            }
        }

        /// <summary>
        /// Vrátí nebo nastaví jméno této komponenty
        /// </summary>
        [DefaultValue(null)]
        public string Name
        {
            get { return name; }
            set
            {
                if (isNameImmutable)
                    throw new ArgumentException("Property jméno je immutable pro tuto instanci", "value");
                if (name == value) return;
                name = value;
                OnPropertyChanged("Name");
            }
        }

        [DefaultValue(null)]
        public object Tag
        {
            get
            {
                return tag;
            }
            set
            {
                if (ReferenceEquals(tag, value)) return;
                tag = value;
                OnPropertyChanged("Tag");
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
