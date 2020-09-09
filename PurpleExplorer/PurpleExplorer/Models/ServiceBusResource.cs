using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ReactiveUI;

namespace PurpleExplorer.Models
{
    public class ServiceBusResource : ReactiveObject
    {
        private string name;
        public ObservableCollection<ServiceBusTopic> Topics { get; set; }

        public string Name
        {
            get => name;

            set => this.RaiseAndSetIfChanged(ref name, value);
        }
    }
}