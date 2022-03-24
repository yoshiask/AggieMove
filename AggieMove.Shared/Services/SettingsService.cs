using OwlCore.AbstractStorage;
using OwlCore.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AggieMove.Services
{
    public class SettingsService : SettingsBase
    {
        public SettingsService(IAsyncSerializer<Stream> settingSerializer)
            : base(folder, settingSerializer)
        {
        }
    }
}
