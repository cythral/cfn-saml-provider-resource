
// [<SetUp>]
// let SetupS3 () =

//     s3Client <- Substitute.For<IAmazonS3>()

//     let bytes = Encoding.UTF8.GetBytes(metadataDoc)
//     let stream = new MemoryStream(bytes)

//     let response =
//         new GetObjectResponse(ResponseStream = stream)

//     s3Client.GetObjectAsync(Arg.Any<GetObjectRequest>()).Returns(response)
//     |> ignore

//     s3Factory <- Substitute.For<S3Factory>()
//     SetPrivateField(samlProvider, "s3Factory", s3Factory)

//     s3Factory.Create(Arg.Any<string>()).Returns(s3Client)
//     |> ignore

// [<SetUp>]
// let SetupS3Uri () =
//     s3Uri <- downcast FormatterServices.GetUninitializedObject(typeof<AmazonS3Uri>)
//     SetPrivateProperty(s3Uri, "Bucket", bucket)
//     SetPrivateProperty(s3Uri, "Key", key)
//     SetPrivateProperty(s3Uri, "Region", region)

//     s3UriFactory <- Substitute.For<S3UriFactory>()
//     SetPrivateField(samlProvider, "s3UriFactory", s3UriFactory)

//     s3UriFactory.Create(Arg.Any<string>()).Returns<AmazonS3Uri>(s3Uri)
//     |> ignore

// [<Test>]
// let ``An s3 client is created with the requested role arn`` () =
//     toTask
//     <| async {
//         let s3Role = "s3RoleArn"

//         samlProvider.Request <- new Request<SamlProvider.Properties>()
//         samlProvider.Request.ResourceProperties <- SamlProvider.Properties(DownloaderRoleArn = s3Role)
//         samlProvider.Create() |> Async.AwaitTask |> ignore

//         s3Factory.Received().Create(Arg.Is<string>(s3Role))
//         |> ignore
//         ()
//        }

// [<Test>]
// let ``An s3 uri is created with the requested metadata document location`` () =
//     toTask
//     <| async {
//         let s3Uri = "s3Uri"

//         samlProvider.Request <- new Request<SamlProvider.Properties>()
//         samlProvider.Request.ResourceProperties <- SamlProvider.Properties(SamlMetadataDocumentLocation = s3Uri)
//         samlProvider.Create() |> Async.AwaitTask |> ignore

//         s3UriFactory.Received().Create(Arg.Is<string>(s3Uri))
//         |> ignore
//         ()
//        }