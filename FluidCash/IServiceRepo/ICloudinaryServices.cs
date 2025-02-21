using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IServiceRepo;

public interface ICloudinaryServices
{
    Task<StandardResponse<DocumentUploadResponse>>
        UploadFileToCloudinaryAsync
        (IFormFile request);

    Task<StandardResponse<string>>
        DeleteFileFromCloudinaryAsync
        (string filePublicId);
}

