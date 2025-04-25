using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IExternalServicesRepo;

public interface ICloudinaryServices
{
    Task<StandardResponse<ICollection<DocumentUploadResponse>>>
        UploadFilesToCloudinaryAsync(IFormFileCollection filesForUpload);

    Task<StandardResponse<DocumentUploadResponse>>
        UploadFileToCloudinaryAsync
        (IFormFile request);

    Task<StandardResponse<string>>
        DeleteFileFromCloudinaryAsync
        (string filePublicId);
}

