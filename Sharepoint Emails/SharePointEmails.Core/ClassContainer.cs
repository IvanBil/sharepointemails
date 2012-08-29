﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharePointEmails.Logging;
using Microsoft.Practices.Unity;
using SharePointEmails.Core.Configuration;

namespace SharePointEmails.Core
{
    public class ClassContainer
    {
        static ClassContainer _Instance = null;

        static object lockObj=new object();

        public static ILogger mockLogger;

        public static ClassContainer Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (lockObj)
                    {
                        if (_Instance == null)
                        {
                            _Instance = new ClassContainer();
                        }
                    }
                }
                return _Instance;
            }
        }

        private UnityContainer m_Container = null;

        private void Register(UnityContainer container)
        {
            if (mockLogger == null)
            {
                container.RegisterType<ILogger, DefaultLogger>(new ContainerControlledLifetimeManager());
            }
            else
            {
                container.RegisterInstance<ILogger>(mockLogger);
            }
            container.RegisterType<IConfigurationManager, ConfigurationManager>();
            container.RegisterType<ITemplatesManager, DefaultTemplatesManager>();
            container.RegisterType<ISiteManager, SiteManager>();
            container.RegisterType<SubstitutionManager, SubstitutionManager>();
        }

        private UnityContainer Container
        {
            get
            {
                if (m_Container == null)
                {
                    lock (lockObj)
                    {
                        if (m_Container==null)
                        {
                            m_Container = new UnityContainer();
                            Register(m_Container);
                        }
                    }
                }
                return m_Container;
            }
        }

        public T Resolve<T>()
        {
            return Container.Resolve<T>();
        }
    }
}
