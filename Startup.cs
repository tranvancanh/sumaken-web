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

            // �N�b�L�[�F�؂ɕK�v�ȃT�[�r�X��o�^
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {

                            // ���_�C���N�g���郍�O�C��URL���������ɕς���
                            // ~/Account/Login =�� ~/account/login
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

            // MVC�ŗ��p����T�[�r�X��o�^
            services.AddMvc(options =>
            {
                            // �O���[�o���t�B���^�ɏ��F�t�B���^��ǉ�
                            // ���ׂẴR���g���[���Ń��O�C�����K�v�ɂ��Ă���
                            var policy = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
                options.EnableEndpointRouting = false;

            });
            services.AddSession();
            services.Configure<RouteOptions>(options =>
            {
                            // URL�͏������ɂ���
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

            //// �p�C�v���C���ɔF�؂̃~�h���E�F�A��ǉ�����
            //// HttpContext.User���Z�b�g���Ă����
            //app.UseAuthentication();


            //app.UseCookiePolicy();

            // Cookie�̌����@�\��L���ɂ���
            app.UseCookiePolicy();
            // ID��L���ɂ���
            app.UseAuthentication();
            //�F�؋@�\��L���ɂ��܂�
            app.UseAuthorization();
            //������3�̑O��̏������t�ɂ��邱�Ƃ͂ł��܂���


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
