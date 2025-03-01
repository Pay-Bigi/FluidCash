namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record ResetPasswordWIthOtpParams
(
string userEmail,
string newPassword,
string otp
);
