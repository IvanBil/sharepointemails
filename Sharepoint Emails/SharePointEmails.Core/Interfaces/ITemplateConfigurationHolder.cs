﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharePointEmails.Core.Interfaces
{
    public interface ITemplateConfigurationHolder
    {
        string[] Data { get; }
    }
}
