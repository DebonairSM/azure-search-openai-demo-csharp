﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MinimalApi.Services;
using NSubstitute;
using Shared.Models;

namespace MinimalApi.Tests;
public class ReadRetrieveReadChatServiceTest
{
    [EnvironmentVariablesFact("AZURE_OPENAI_ENDPOINT", "AZURE_OPENAI_EMBEDDING_DEPLOYMENT", "AZURE_OPENAI_CHATGPT_DEPLOYMENT")]
    public async Task NorthwindHealthQuestionTest_TextOnlyAsync()
    {
        var documentSearchService = Substitute.For<ISearchService>();
        documentSearchService.QueryDocumentsAsync(Arg.Any<string?>(), Arg.Any<float[]?>(), Arg.Any<RequestOverrides?>(), Arg.Any<CancellationToken>())
                .Returns(new SupportingContentRecord[]
                {
                    new SupportingContentRecord("Northwind_Health_Plus_Benefits_Details-52.pdf", "The Northwind Health Plus plan covers a wide range of services related to the treatment of SUD. These services include inpatient and outpatient treatment, counseling, and medications to help with recovery. It also covers mental health services and support for family members of those with SUD"),
                    new SupportingContentRecord("Northwind_Health_Plus_Benefits_Details-90.pdf", "This contract includes the plan documents that you receive from Northwind Health, the Northwind Health Plus plan summary, and any additional contracts or documents that you may have received from Northwind Health. It is important to remember that any changes made to this plan must be in writing and signed by both you and Northwind Health."),
                });

        var openAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException();
        var openAIClient = new OpenAIClient(new Uri(openAIEndpoint), new DefaultAzureCredential());
        var openAiEmbeddingDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBEDDING_DEPLOYMENT") ?? throw new InvalidOperationException();
        var openAIChatGptDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHATGPT_DEPLOYMENT") ?? throw new InvalidOperationException();

        var configuration = Substitute.For<IConfiguration>();
        configuration["AzureOpenAiChatGptDeployment"].Returns(openAIChatGptDeployment);
        configuration["AzureOpenAiEmbeddingDeployment"].Returns(openAiEmbeddingDeployment);
        configuration["AzureOpenAiServiceEndpoint"].Returns(openAIEndpoint);
        configuration["AzureStorageAccountEndpoint"].Returns("https://northwindhealth.blob.core.windows.net/");
        configuration["AzureStorageContainer"].Returns("northwindhealth");

        var chatService = new ReadRetrieveReadChatService(documentSearchService, openAIClient, configuration);

        var history = new ChatTurn[]
        {
            new ChatTurn("What is included in my Northwind Health Plus plan that is not in standard?", "user"),
        };
        var overrides = new RequestOverrides
        {
            RetrievalMode = RetrievalMode.Text,
            Top = 2,
            SemanticCaptions = true,
            SemanticRanker = true,
            SuggestFollowupQuestions = true,
        };

        var response = await chatService.ReplyAsync(history, overrides);

        // TODO
        // use AutoGen agents to evaluate if answer
        // - has follow up question
        // - has correct answer
        // - has has correct format for source reference.

        response.DataPoints.Count().Should().Be(2);
        response.Answer.Should().NotBeNullOrEmpty();
        response.CitationBaseUrl.Should().Be("https://northwindhealth.blob.core.windows.net/northwindhealth");
    }
}
