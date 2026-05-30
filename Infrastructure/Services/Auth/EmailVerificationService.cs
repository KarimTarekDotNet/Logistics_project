using Application.DTOs.Auth;
using Application.Interfaces.Services.Auth;
using Domain.Entities.Users;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Infrastructure.Services.Auth
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private const string FrontendBaseUrl = "https://karimtarekdotnet.github.io/Logistics_project";
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmailVerificationService(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task<AuthResponse> SendEmailConfirmationAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return new AuthResponse { Id = userId, IsAuthenticated = false, Message = "User not found." };

            if (string.IsNullOrWhiteSpace(user.Email))
                return new AuthResponse { Id = user.Id, IsAuthenticated = false, Message = "User email is not configured." };

            if (user.EmailConfirmed)
                return new AuthResponse { Id = user.Id, IsAuthenticated = true, Message = "Email is already confirmed." };

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encoded = WebUtility.UrlEncode(token);
            var confirmationLink = $"{FrontendBaseUrl}/confirm-email?userId={user.Id}&token={encoded}";


            var subject = "Confirm Your Email Address";

            var body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                    </head>
                    <body style='font-family: Arial, sans-serif; background-color:#f6f8fb; padding: 20px;'>
                        <div style='max-width:600px; margin:auto; background:#ffffff; padding:30px; border-radius:10px; border:1px solid #e5e7eb;'>
        
                            <h2 style='color:#111827; margin-bottom:10px;'>Confirm Your Email Address</h2>

                            <p style='color:#374151; font-size:15px;'>
                                Hello {user.UserName},
                            </p>

                            <p style='color:#374151; font-size:15px; line-height:1.6;'>
                                Thank you for registering with <strong>Logistics System</strong>.
                                Please confirm your email address to activate your account and continue using our services.
                            </p>

                            <p style='margin:30px 0;'>
                                <a href='{confirmationLink}'
                                   style='background-color:#2563eb; color:#ffffff; padding:12px 20px; text-decoration:none; border-radius:8px; display:inline-block;'>
                                    Confirm Email
                                </a>
                            </p>

                            <p style='color:#374151; font-size:14px; line-height:1.6;'>
                                If the button does not work, copy and paste this link into your browser:
                            </p>

                            <p style='word-break:break-all; color:#2563eb; font-size:13px;'>
                                {confirmationLink}
                            </p>

                            <p style='color:#374151; font-size:15px; line-height:1.6;'>
                                If you did not create this account, you can safely ignore this email.
                            </p>

                            <p style='color:#6b7280; font-size:13px; margin-top:25px;'>
                                For your security, never share confirmation links or verification codes with anyone.
                            </p>

                            <hr style='border:none; border-top:1px solid #e5e7eb; margin:25px 0;' />

                            <p style='color:#6b7280; font-size:13px;'>
                                Best regards,<br/>
                                <strong>Logistics Team</strong>
                            </p>
                        </div>
                    </body>
                    </html>";

            await _emailSender.SendEmailAsync(user.Email, subject, body);

            return new AuthResponse
            {
                Id = user.Id,
                IsAuthenticated = false,
                Message = "Email confirmation link has been sent successfully."
            };
        }

        public async Task<AuthResponse> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return new AuthResponse { Id = userId, IsAuthenticated = false, Message = "User not found." };

            if (string.IsNullOrWhiteSpace(token))
                return new AuthResponse { Id = user.Id, IsAuthenticated = false, Message = "Invalid confirmation token." };

            if (user.EmailConfirmed)
                return new AuthResponse { Id = user.Id, IsAuthenticated = true, Message = "Email is already confirmed." };

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                return new AuthResponse
                {
                    Id = user.Id,
                    IsAuthenticated = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            return new AuthResponse
            {
                Id = user.Id,
                IsAuthenticated = true,
                Message = "Email confirmed successfully."
            };
        }

        public async Task<AuthResponse> ResendEmailConfirmationAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new AuthResponse
                {
                    Email = email,
                    IsAuthenticated = false,
                    Message = "Email is required."
                };
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new AuthResponse
                {
                    Email = email,
                    IsAuthenticated = false,
                    Message = "If this email exists, a confirmation link has been sent."
                };
            }

            if (user.EmailConfirmed)
            {
                return new AuthResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    IsAuthenticated = true,
                    Message = "Email is already confirmed."
                };
            }

            return await SendEmailConfirmationAsync(user.Id);
        }

        public async Task SendChangeEmailConfirmationAsync(string userId, string newEmail)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new BusinessRuleException("User not found.");

            if (string.IsNullOrWhiteSpace(newEmail))
                throw new BusinessRuleException("New email is required.");

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var encodedToken = WebUtility.UrlEncode(token);

            var confirmationLink =
                $"{FrontendBaseUrl}/confirm-email-change?userId={user.Id}&token={encodedToken}";

            var subject = "Confirm Your New Email Address";

            var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
            </head>
            <body style='font-family: Arial, sans-serif; background-color:#f6f8fb; padding:20px;'>
                <div style='max-width:600px; margin:auto; background:#ffffff; padding:30px; border-radius:10px; border:1px solid #e5e7eb;'>

                    <h2 style='color:#111827;'>Confirm Your New Email</h2>

                    <p style='color:#374151;'>
                        Hello {user.UserName},
                    </p>

                    <p style='color:#374151; line-height:1.6;'>
                        You requested to change your email address.
                        Please confirm your new email to complete the update.
                    </p>

                    <p style='margin:30px 0;'>
                        <a href='{confirmationLink}'
                           style='background-color:#2563eb; color:#ffffff; padding:12px 20px; text-decoration:none; border-radius:8px;'>
                            Confirm New Email
                        </a>
                    </p>

                    <p style='color:#6b7280; font-size:13px;'>
                        If you did not request this change, you can safely ignore this email.
                    </p>

                    <hr style='margin:25px 0;' />

                    <p style='color:#6b7280; font-size:13px;'>
                        Logistics System Team
                    </p>
                </div>
            </body>
            </html>";

            await _emailSender.SendEmailAsync(newEmail, subject, body);
        }
    }
}
