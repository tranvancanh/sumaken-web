using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using stock_management_system.Filters;
using stock_management_system.Models.common;
using stock_management_system.common;

namespace stock_management_system
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddControllersWithViews();
#if DEBUG
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
#else
            services.AddControllersWithViews();
#endif
            services.Configure<CookiePolicyOptions>(options =>
                        {
                            options.CheckConsentNeeded = context => !context.User.Identity.IsAuthenticated;
                            options.MinimumSameSitePolicy = SameSiteMode.None;
                            options.Secure = CookieSecurePolicy.Always;
                            options.HttpOnly = HttpOnlyPolicy.Always;
                        });

            // クッキー認証に必要なサービスを登録
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {

                            // リダイレクトするログインURLも小文字に変える
                            // ~/Account/Login =＞ ~/account/login
                options.LoginPath = CookieAuthenticationDefaults.LoginPath.ToString().ToLower();
                //options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.Name = CookieAuthenticationDefaults.AuthenticationScheme;
                //options.Cookie.MaxAge = TimeSpan.FromMinutes(1440);
                options.LoginPath = "/Account/index";
                options.SlidingExpiration = false;
                //options.ExpireTimeSpan = TimeSpan.FromMinutes(1440);

            });

            //services.Configure<IdentityOptions>(options =>
            //{
            //    // Password settings.
            //    options.Password.RequireDigit = true;
            //    options.Password.RequireLowercase = true;
            //    options.Password.RequireNonAlphanumeric = true;
            //    options.Password.RequireUppercase = true;
            //    options.Password.RequiredLength = 6;
            //    options.Password.RequiredUniqueChars = 1;

            //    // Lockout settings.
            //    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
            //    options.Lockout.MaxFailedAccessAttempts = 5;
            //    options.Lockout.AllowedForNewUsers = true;

            //    //// User settings.
            //    //options.User.AllowedUserNameCharacters =
            //    //"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            //    //options.User.RequireUniqueEmail = false;
            //});

            // MVCで利用するサービスを登録
            services.AddMvc(options =>
            {
                            // グローバルフィルタに承認フィルタを追加
                            // すべてのコントローラでログインが必要にしておく
                            var policy = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
                options.EnableEndpointRouting = false;

            });
            services.AddSession();
            services.Configure<RouteOptions>(options =>
            {
                            // URLは小文字にする
                            options.LowercaseUrls = true;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("1", policy =>
                {
                    policy.RequireClaim(CustomClaimTypes.ClaimType_Role, "1");
                });
                options.AddPolicy("test", policy =>
                {
                    policy.RequireClaim(CustomClaimTypes.ClaimType_Role, "test");
                });
            });

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(typeof(MyFilter));
            });

            //services.AddWebOptimizer(pipeline =>
            //{
            //    // Creates a CSS and a JS bundle. Globbing patterns supported.
            //    pipeline.AddCssBundle("/css/bundle.css", "/css/*.css", "/vendor/**/*.css");
            //    pipeline.AddJavaScriptBundle("/js/bundle.js", "js/*.js", "vandor/*.js");
            //});

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseWebOptimizer();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //app.UseAuthorization();

            //// パイプラインに認証のミドルウェアを追加する
            //// HttpContext.Userをセットしてくれる
            //app.UseAuthentication();


            //app.UseCookiePolicy();

            // Cookieの原則機能を有効にする
            app.UseCookiePolicy();
            // IDを有効にする
            app.UseAuthentication();
            //認証機能を有効にします
            app.UseAuthorization();
            //これらの3つの前後の順序を逆にすることはできません


            app.UseSession();
            app.UseMvc();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
