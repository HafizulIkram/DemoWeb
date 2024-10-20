﻿namespace DemoWeb.Data
{
    using NHibernate;
    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;
    using NHibernate.Tool.hbm2ddl;
    using DemoWeb.Mapping;

    public class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;
        //create session object that are used to connect the object with the database
       
        public ISessionFactory SessionFactory 
        {
            get
            {
                if (_sessionFactory == null)
                {
                    /*var cfg = new Configuration();
                    cfg.DataBaseIntegration(x =>
                    {
                        x.ConnectionString = _configuration.GetConnectionString("DefaultConnection");
                        x.Driver<SqlClientDriver>();
                        x.Dialect<MsSql2012Dialect>();
                    });

                    cfg.AddAssembly(Assembly.GetExecutingAssembly());

                    cfg.SetProperty(NHibernate.Cfg.Environment.Hbm2ddlAuto, "update");*/

                    var database = Fluently.Configure()
                        .Database(MsSqlConfiguration.MsSql2008.FormatSql().ConnectionString("Server=Ik_LAP\\SQLEXPRESS;Database=EmployeeDB;Trusted_Connection=True;"))
                        .Mappings(x =>
                        {
                            x.FluentMappings.AddFromAssemblyOf<EmployeeMap>();
                            x.FluentMappings.AddFromAssemblyOf<TasksMap>();
                        });


                    _sessionFactory = database.BuildSessionFactory();
                }
                return _sessionFactory;
            }
        }

        // Responsible for query the database
        public ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}
