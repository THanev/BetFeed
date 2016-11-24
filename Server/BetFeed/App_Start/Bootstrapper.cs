﻿using Autofac;
using Autofac.Integration.WebApi;
using BetFeed.Infrastructure;
using BetFeed.Infrastructure.Repository;
using BetFeed.Models;
using BetFeed.Services;
using System.Data.Entity;
using System.Reflection;
using System.Web.Http;
using System;
using BetFeed.ViewModels;
using AutoMapper;

namespace BetFeed.App_Start
{
    public static class Bootstrapper
    {
        // This class breaks single responsibility, extract those 2 methods in different classes :
        // - AutoFacCondif
        // - AutoMapperConfig
        // Then call the static methods in global.asax
        public static void Run()
        {
            SetAutofacContainer();
            RegisterAutoMapper();
        }

        private static void RegisterAutoMapper()
        {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Sport, SportViewModel>()
                    .ForMember(dest => dest.Categories, opt => opt.Ignore());

                cfg.CreateMap<Event, EventViewModel>()
                    .ForMember(dest => dest.MatchCount, opt => opt.Ignore());
                cfg.CreateMap<Event, NewMatchesViewModel>()
                    .ForMember(dest => dest.RequestDate, opt => opt.Ignore());

                cfg.CreateMap<Match, MatchViewModel>();
                cfg.CreateMap<Bet, BetViewModel>();
                cfg.CreateMap<Odd, OddViewModel>();

                cfg.CreateMap<Match, MatchWithBetsViewModel>()
                    .ForMember(dest => dest.First, opt => opt.MapFrom(match => match.Name.Split('-')[0]))
                    .ForMember(dest => dest.Second, opt => opt.MapFrom(match => match.Name.Split('-')[1]));

                cfg.CreateMap<Sport, SportWithNameAndId>()
                    .ForMember(dest => dest.SportId, opt => opt.MapFrom(sport => sport.Id))
                    .ForMember(dest => dest.SportName, opt => opt.MapFrom(sport => sport.Name))
                    .ForMember(dest => dest.EventsCount, opt => opt.MapFrom(sport => sport.Events.Count));

                cfg.CreateMap<Event, EventWithMatchesViewModel>()
                    .ForMember(dest => dest.RequestDate, opt => opt.Ignore());
            });

            Mapper.AssertConfigurationIsValid();
        }

        private static void SetAutofacContainer()
        {
            var builder = new ContainerBuilder();

            // Get HttpConfiguration.
            var config = GlobalConfiguration.Configuration;

            // Register Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // OPTIONAL: Register the Autofac filter provider.
            builder.RegisterWebApiFilterProvider(config);

            // Register DbContext
            builder.RegisterType<BetFeedContext>().AsSelf();

            // Register generic repository
            builder.RegisterAssemblyTypes(typeof(IRepository<Sport>).Assembly)
                .Where(t => t.Name.Equals("IRepository"))
                .AsImplementedInterfaces()
                .InstancePerRequest();

            //Register custom repositories
            builder.RegisterType<SportRepository>().As<IRepository<Sport>>();
            builder.RegisterType<EfRepository<Event>>().As<IRepository<Event>>();
            builder.RegisterType<EfRepository<Bet>>().As<IRepository<Bet>>();
            builder.RegisterType<MatchRepository>().As<IRepository<Match>>();
            builder.RegisterType<EfRepository<Odd>>().As<IRepository<Odd>>();

            // Register Services
            builder.RegisterAssemblyTypes(typeof(VitalbetService).Assembly)
               .Where(t => t.Name.EndsWith("Service"))
               .AsImplementedInterfaces()
               .InstancePerRequest();

            // Set the dependency resolver to be Autofac.
            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}