using FluidCash.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FluidCash.DataAccess.DataConfigs;

public class DefaultEmailTemplateSeeding : IEntityTypeConfiguration<EmailTemplate>
{
    public void
        Configure
        (EntityTypeBuilder<EmailTemplate> modelBuilder)
    {
        modelBuilder.HasData
        (
            new EmailTemplate
            {
                Id = "defaultId",
                CreatedAt = DateTime.UtcNow.ToUniversalTime(),
                CreatedBy = "Admin",
                IsDeleted = false,
                TemplateName = "Default Mail Template",
                Template = @"<!DOCTYPE html>
                                        <html lang=""en"">

                                        <head>
                                          <meta charset=""UTF-8"">
                                          <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                          <title>Email Template</title>
                                          <style>
                                            @import url(https://cdn.jsdelivr.net/npm/@xz/fonts@1/serve/hk-grotesk.min.css);

                                            body {
                                              font-size: 16px;
                                              background: #f6f6f5;
                                              font-family: ""HK Grotesk"", sans-serif;
                                            }

                                            p {
                                              margin-top: 20px;
                                              margin-bottom: 24px;
                                              line-height: 1.5;
                                              text-align: justify;
                                            }

                                            table {
                                              width: 100%;
                                            }

                                            a {
                                              color: #000000;
                                              font-weight: 600;
                                              text-decoration: none;
                                            }

                                            img {
                                              width: 100%;
                                              height: auto;
                                            }

                                            .wrapper {
                                              width: 100%;
                                              max-width: 567px;
                                              margin: 32px auto;
                                            }

                                            .header {
                                              padding: 24px 32px;
                                            }

                                            .content {
                                              padding: 20px 32px;
                                              background-color: #ffffff;
                                            }

                                            .footer {
                                              padding: 10px 32px;
                                              background-color: #000000;
                                              color: #D80944;
                                              font-size: 14px;
                                              font-weight: 300;
                                              line-height: 1.6;
                                            }

                                            .footer a {
                                              text-decoration: none;
                                              color: #D80944;
                                              font-weight: 600;
                                            }

                                            .footer a:hover {
                                              color: #D80944;
                                            }

                                            .footer .logo {
                                              width: 150px;
                                            }

                                            .footer .navigation {
                                              text-align: center;
                                              margin-bottom: 10px;
                                            }

                                            .footer .navigation a {
                                              display: inline-block;
                                              color: #ffffff;
                                              margin: 0 10px;
                                            }

                                            .footer .navigation a:hover {
                                              color: #D80944;
                                            }

                                            .footer .social-icons {
                                              text-align: center;
                                              margin-bottom: 10px;
                                            }

                                            .footer .social-icons img {
                                              width: 24px;
                                              height: 24px;
                                              margin: 0 5px;
                                            }

                                            .footer .social-icons a:hover img {
                                              filter: invert(29%) sepia(84%) saturate(4849%) hue-rotate(340deg) brightness(95%) contrast(102%);
                                            }

                                            .footer .legal {
                                              text-align: right;
                                            }

                                            .footer .legal a {
                                              color: #777777;
                                            }

                                            .footer .legal a:hover {
                                              color: #D80944;
                                            }

                                            .footer .contact {
                                              color: #777777;
                                              text-align: right;
                                            }

                                            .footer .contact a:hover {
                                              color: #D80944;
                                            }

                                            .footer .separator {
                                              border-top: 1px solid #dddddd;
                                              margin: 10px 0;
                                            }
                                          </style>
                                        </head>

                                        <body>
                                          <div class=""wrapper"">
                                            <!-- Email Header -->
                                            <div class=""header"">
                                              <a href=""{{org-url}}"">
                                                <img class=""logo"" src=""{{logo}}"" alt="""" />
                                              </a>
                                            </div>
                                            <!-- End Email Header -->

                                            <!-- Email Content -->
                                            <div class=""content"">
                                              <table>
                                                <tr>
                                                  <td>
                                                    <p><strong>Hello {{recipient}},</strong></p>
                                                  </td>
                                                </tr>
                                                <tr>
                                                  <td>
                                                    {{body}}
                                                  </td>
                                                </tr>
                                              </table>
                                            </div>
                                            <!-- End Email Content -->

                                            <!-- Email Footer -->
                                            <div class=""footer"">
                                              <table>
                                                <tr>
                                                  <td style=""width: 25%;"">
                                                    <a href=""{{org-url}}"">
                                                      <img class=""logo"" src=""{{logo}}"" alt=""Company Logo"" />
                                                    </a>
                                                  </td>
                                                  <td style=""width: 50%; text-align: center;"">
                                                    <div class=""navigation"">
                                                      <a href=""{{org-url}}"">About Us</a>
                                                      <a href=""{{org-url}}"">Products</a>
                                                      <a href=""{{org-url}}"">Support</a>
                                                      <a href=""{{org-url}}"">Blog</a>
                                                    </div>
                                                    <div class=""social-icons"">
                                                      <a href=""{{linkedinurl}}"">
                                                        <img src=""https://cdn-icons-png.flaticon.com/512/174/174857.png"" alt=""LinkedIn"" />
                                                      </a>
                                                      <a href=""{{twitterurl}}"">
                                                        <img src=""https://cdn-icons-png.flaticon.com/512/733/733579.png"" alt=""Twitter"" />
                                                      </a>
                                                    </div>
                                                  </td>
                                                  <td style=""width: 25%; text-align: right;"">
                                                    <div class=""legal"">
                                                      <a href=""{{org-url}}"">Terms of Use</a>
                                                      <br />
                                                      <a href=""{{org-url}}"">Privacy Policy</a>
                                                    </div>
                                                    <div class=""contact"">
                                                      <p>Email: <a href=""mailto:{{sendermail}}"">{{sendermail}}</a></p>
                                                    </div>
                                                  </td>
                                                </tr>
                                                <tr>
                                                  <td colspan=""3"">
                                                    <div class=""separator""></div>
                                                    <p style=""text-align: center; color: #777777;"">
                                                      © {{year}} {{org-name}}. All Rights Reserved.
                                                    </p>
                                                  </td>
                                                </tr>
                                              </table>
                                            </div>
                                            <!-- End Email Footer -->
                                          </div>
                                        </body>

                                        </html>"
            }
        );

    }
}