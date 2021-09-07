﻿using FluentValidation;
using KITT.Core.Commands;
using KITT.Core.Models;
using KITT.Core.Persistence;
using KITT.Core.Test.Fixtures;
using KITT.Core.Validators;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace KITT.Core.Test.Commands
{
    public class StreamingCommandsTest : IClassFixture<StreamingCommandsFixture>
    {
        private readonly StreamingCommandsFixture _fixture;

        public StreamingCommandsTest(StreamingCommandsFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        #region Constructor tests
        [Fact]
        public void Ctor_Should_Throw_ArgumentNullException_If_Context_Is_Null()
        {
            KittDbContext context = null;
            var validator = new StreamingValidator(_fixture.Context);

            var ex = Assert.Throws<ArgumentNullException>(() => new StreamingCommands(context, validator));
            Assert.Equal(nameof(context), ex.ParamName);
        }

        [Fact]
        public void Ctor_Should_Throw_ArgumentNullException_If_Validator_Is_Null()
        {
            KittDbContext context = _fixture.Context;
            StreamingValidator validator = null;

            var ex = Assert.Throws<ArgumentNullException>(() => new StreamingCommands(context, validator));
            Assert.Equal(nameof(validator), ex.ParamName);
        }
        #endregion

        #region ScheduleStreamingAsync tests
        [Fact]
        public async Task ScheduleStreamingAsync_Should_Throw_ValidationException_If_Streaming_Slug_Already_Exists()
        {
            _fixture.PrepareData(context =>
            {
                var streaming = Streaming.Schedule(
                    "test1", 
                    "test-slug", 
                    "albx87", 
                    DateTime.Today.AddDays(1), 
                    TimeSpan.FromHours(16), 
                    TimeSpan.FromHours(18), 
                    "https://www.twitch.tv/albx87");

                context.Add(streaming);
                context.SaveChanges();
            });

            var validator = new StreamingValidator(_fixture.Context);
            var commands = new StreamingCommands(_fixture.Context, validator);

            string twitchChannel = "albx87";
            string streamingTitle = "test";
            string streamingSlug = "test-slug";
            DateTime scheduleDate = DateTime.Today.AddDays(2);
            TimeSpan startingTime = TimeSpan.FromHours(19);
            TimeSpan endingTime = TimeSpan.FromHours(20);
            string hostingChannelUrl = "https://www.twitch.tv/albx87";
            string streamingAbstract = "streaming abstract";

            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => commands.ScheduleStreamingAsync(
                    twitchChannel,
                    streamingTitle,
                    streamingSlug,
                    scheduleDate,
                    startingTime,
                    endingTime,
                    hostingChannelUrl,
                    streamingAbstract));

            Assert.Contains(nameof(Streaming.Slug), ex.Errors.Select(e => e.PropertyName));
        }

        [Fact]
        public async Task ScheduleStreamingAsync_Should_Add_Streaming_With_Specified_Values()
        {
            var validator = new StreamingValidator(_fixture.Context);
            var commands = new StreamingCommands(_fixture.Context, validator);

            string twitchChannel = "albx87";
            string streamingTitle = "test";
            string streamingSlug = "test-schedule-streaming-slug";
            DateTime scheduleDate = DateTime.Today;
            TimeSpan startingTime = TimeSpan.FromHours(16);
            TimeSpan endingTime = TimeSpan.FromHours(18);
            string hostingChannelUrl = "https://www.twitch.tv/albx87";
            string streamingAbstract = "streaming abstract";

            var scheduledStreamingId = await commands.ScheduleStreamingAsync(
                twitchChannel,
                streamingTitle,
                streamingSlug,
                scheduleDate,
                startingTime,
                endingTime,
                hostingChannelUrl,
                streamingAbstract);

            var scheduledStreaming = _fixture.Context.Streamings.FirstOrDefault(s => s.Id == scheduledStreamingId);

            Assert.NotNull(scheduledStreaming);
            Assert.Equal(twitchChannel, scheduledStreaming.TwitchChannel);
            Assert.Equal(streamingTitle, scheduledStreaming.Title);
            Assert.Equal(streamingSlug, scheduledStreaming.Slug);
            Assert.Equal(scheduleDate, scheduledStreaming.ScheduleDate);
            Assert.Equal(startingTime, scheduledStreaming.StartingTime);
            Assert.Equal(endingTime, scheduledStreaming.EndingTime);
            Assert.Equal(hostingChannelUrl, scheduledStreaming.HostingChannelUrl);
            Assert.Equal(streamingAbstract, scheduledStreaming.Abstract);
        }
        #endregion

        #region UpdateStreamingAsync tests
        [Fact]
        public async Task UpdateStreamingAsync_Should_Update_Streaming_Information_Correctly()
        {
            var streamingId = Guid.Empty;

            var validator = new StreamingValidator(_fixture.Context);
            var commands = new StreamingCommands(_fixture.Context, validator);

            _fixture.PrepareData(context =>
            {
                var newStreaming = Streaming.Schedule(
                    "title",
                    "slug",
                    "albx87",
                    DateTime.Today,
                    TimeSpan.FromHours(19),
                    TimeSpan.FromHours(20),
                    "https://www.twitch.tv/albx87");

                context.Add(newStreaming);
                context.SaveChanges();

                streamingId = newStreaming.Id;
            });

            string streamingTitle = "new title";
            DateTime scheduleDate = DateTime.Today.AddDays(2);
            TimeSpan startingTime = TimeSpan.FromHours(20);
            TimeSpan endingTime = TimeSpan.FromHours(21);
            string hostingChannelUrl = "https://www.twitch.tv/newfakechannel";
            string streamingAbstract = "new abstract";
            string youtubeRegistrationLink = "https://www.youtube.com";

            await commands.UpdateStreamingAsync(
                streamingId,
                streamingTitle,
                scheduleDate,
                startingTime,
                endingTime,
                hostingChannelUrl,
                streamingAbstract,
                youtubeRegistrationLink);

            var updatedStreaming = _fixture.Context.Streamings.SingleOrDefault(s => s.Id == streamingId);

            Assert.Equal(streamingTitle, updatedStreaming.Title);
            Assert.Equal(scheduleDate, updatedStreaming.ScheduleDate);
            Assert.Equal(startingTime, updatedStreaming.StartingTime);
            Assert.Equal(endingTime, updatedStreaming.EndingTime);
            Assert.Equal(hostingChannelUrl, updatedStreaming.HostingChannelUrl);
            Assert.Equal(streamingAbstract, updatedStreaming.Abstract);
            Assert.Equal(youtubeRegistrationLink, updatedStreaming.YouTubeVideoUrl);
        }
        #endregion

        #region DeleteStreamingAsync tests
        #endregion
    }
}
