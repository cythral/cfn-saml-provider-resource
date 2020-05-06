using System.IO;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using static System.Text.Json.JsonSerializer;

using Amazon.S3.Model;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;

using Cythral.CloudFormation.Resources.Aws;

using Cythral.CloudFormation.CustomResource.Attributes;
using Cythral.CloudFormation.CustomResource.Core;

namespace Cythral.CloudFormation.Resources
{
    [CustomResource]
    public partial class SamlProvider
    {
        public class Properties
        {
            public string? CreatorRoleArn { get; set; } = null;

            public string? DownloaderRoleArn { get; set; } = null;

            [Required]
            [UpdateRequiresReplacement]
            public string Name { get; set; } = "";

            [Required]
            public string SamlMetadataDocumentLocation { get; set; } = "";
        }

        private IamFactory iamFactory = new IamFactory();

        private S3GetObjectFacadeFactory s3GetObjectFacadeFactory = new S3GetObjectFacadeFactory();

        public async Task<Response> Create()
        {
            var props = Request.ResourceProperties;
            var iamClient = await iamFactory.Create(props.CreatorRoleArn);
            var s3GetObjectFacade = s3GetObjectFacadeFactory.Create();

            var metadataDoc = await s3GetObjectFacade.GetObject(props.SamlMetadataDocumentLocation, props.DownloaderRoleArn);
            var createSamlProviderResponse = await iamClient.CreateSAMLProviderAsync(new CreateSAMLProviderRequest
            {
                Name = props.Name,
                SAMLMetadataDocument = metadataDoc
            });

            Console.WriteLine($"Create SAML Provider Response: {Serialize(createSamlProviderResponse)}");

            return new Response
            {
                PhysicalResourceId = createSamlProviderResponse.SAMLProviderArn
            };
        }

        public async Task<Response> Update()
        {
            var props = Request.ResourceProperties;
            var iamClient = await iamFactory.Create(props.CreatorRoleArn);
            var s3GetObjectFacade = s3GetObjectFacadeFactory.Create();

            var metadataDoc = await s3GetObjectFacade.GetObject(props.SamlMetadataDocumentLocation, props.DownloaderRoleArn);
            var updateSamlProviderResponse = await iamClient.UpdateSAMLProviderAsync(new UpdateSAMLProviderRequest
            {
                SAMLProviderArn = Request.PhysicalResourceId,
                SAMLMetadataDocument = metadataDoc
            });

            Console.WriteLine($"Update SAML Provider Response: {Serialize(updateSamlProviderResponse)}");

            return new Response
            {
                PhysicalResourceId = Request.PhysicalResourceId
            };
        }

        public async Task<Response> Delete()
        {
            var props = Request.ResourceProperties;
            var iamClient = await iamFactory.Create(props.CreatorRoleArn);

            var deleteSamlProviderResponse = await iamClient.DeleteSAMLProviderAsync(new DeleteSAMLProviderRequest
            {
                SAMLProviderArn = Request.PhysicalResourceId
            });

            Console.WriteLine($"Delete SAML Provider Response: {Serialize(deleteSamlProviderResponse)}");

            return new Response
            {
                PhysicalResourceId = Request.PhysicalResourceId
            };
        }
    }
}
