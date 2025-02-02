﻿// Copyright (c) Microsoft. All rights reserved.


namespace EmbedFunctions.Services;

internal sealed class MilvusEmbedService : IEmbedService
{
    public Task CreateSearchIndexAsync(string searchIndexName, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EmbedImageBlobAsync(Stream imageStream, string imageName, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EmbedPDFBlobAsync(Stream blobStream, string blobName) => throw new NotImplementedException();

    public Task EnsureSearchIndexAsync(string searchIndexName, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
