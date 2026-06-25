using System.Collections.Generic;
using Xunit;
using Moq;
using ComtradeViewer.ViewModel.ViewModels;
using ComtradeViewer.Model.Services;
using ComtradeViewer.Model.Models;

namespace ComtradeViewer.Tests
{
    public class MainViewModelTests
    {
        [Fact]
        public void ExecuteOpenFile_WhenCalled_PopulatesChannelsAndSelectsFirst()
        {
            var fakeData = new Dictionary<string, List<SamplePoint>>
            {
                { "Channel_A", new List<SamplePoint>() },
                { "Channel_B", new List<SamplePoint>() }
            };

            var mockParser = new Mock<IComtradeParser>();
            mockParser
                .Setup(p => p.Parse(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fakeData);

            var viewModel = new MainViewModel(mockParser.Object);

            viewModel.OpenFileCommand.Execute(new string[] { "dummy.cfg", "dummy.dat" });
            Assert.Equal(2, viewModel.Channels.Count);
            Assert.Contains("Channel_A", viewModel.Channels);
            Assert.Equal("Channel_A", viewModel.SelectedChannel);
        }

        [Fact]
        public void ChartWidth_WhenChanged_RecalculatesPointsAccordingToDownsampler()
        {
            var rawPoints = new List<SamplePoint>();
            for (int i = 0; i < 100; i++) rawPoints.Add(new SamplePoint(i, i));

            var fakeData = new Dictionary<string, List<SamplePoint>> { { "Test_Ch", rawPoints } };

            var mockParser = new Mock<IComtradeParser>();
            mockParser.Setup(p => p.Parse(It.IsAny<string>(), It.IsAny<string>())).Returns(fakeData);

            var viewModel = new MainViewModel(mockParser.Object);
            
            viewModel.OpenFileCommand.Execute(new string[] { "a", "b" });
            viewModel.SelectedChannel = "Test_Ch";

            viewModel.ChartWidth = 20;

            Assert.Equal(40, viewModel.ChartPoints.Count);
        }
    }
}
