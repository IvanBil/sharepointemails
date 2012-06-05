﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharePointEmails.Core
{
    public interface ITemplatesManager
    {
        Exception LastError
        {
            get;
            set;
        }

        ITemplate GetTemplate(ISearchContext owner, TemplateGettingEnum settings);

        /// <remarks>
        /// Exeptions:
        /// SeWrongOwner
        /// SeBaseException
        /// </remarks>
        void AddTemplate(ITemplate template, ISearchContext owner);

        /// <remarks>
        /// Exceptions
        /// SeBaseException
        /// SeWrongOwnerException
        /// </remarks>
        List<ITemplate> GetAllTemplates(ISearchContext owner);
    }
}
