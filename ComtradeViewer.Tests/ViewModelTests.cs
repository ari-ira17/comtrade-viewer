using System.Collections.Generic;
using Xunit;
using Moq;
using ComtradeViewer.ViewModel.ViewModels;
using ComtradeViewer.ViewModel;
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

        [Fact]
        public void OpenFile_WhenNoChannels_ClearsSelection()
        {
            var mockParser = new Mock<IComtradeParser>();

            mockParser
                .Setup(p => p.Parse(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Dictionary<string, List<SamplePoint>>());

            var viewModel = new MainViewModel(mockParser.Object);

            viewModel.OpenFileCommand.Execute(new[] { "a", "b" });

            Assert.Empty(viewModel.Channels);
            Assert.Null(viewModel.SelectedChannel);
            Assert.Empty(viewModel.ChartPoints);
        }

        [Fact]
        public void OpenFile_IgnoresInvalidParameter()
        {
            var mockParser = new Mock<IComtradeParser>();

            var viewModel = new MainViewModel(mockParser.Object);

            viewModel.OpenFileCommand.Execute("wrong parameter");

            mockParser.Verify(
                p => p.Parse(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void OpenFile_WhenParserThrows_DoesNotThrow()
        {
            var mockParser = new Mock<IComtradeParser>();

            mockParser
                .Setup(p => p.Parse(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Parse error"));

            var viewModel = new MainViewModel(mockParser.Object);

            var ex = Record.Exception(() =>
                viewModel.OpenFileCommand.Execute(new[] { "a", "b" }));

            Assert.Null(ex);
        }

        [Fact]
        public void ChartWidth_LessThanOrEqual10_IsIgnored()
        {
            var viewModel = new MainViewModel();

            double originalWidth = viewModel.ChartWidth;

            viewModel.ChartWidth = 10;

            Assert.Equal(originalWidth, viewModel.ChartWidth);
        }

        [Fact]
        public void RelayCommand_CanExecute_ReturnsTrue_WhenPredicateIsNull()
        {
            var command = new RelayCommand(_ => { });

            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void RelayCommand_CanExecute_UsesPredicate()
        {
            var command = new RelayCommand(
                _ => { },
                _ => false);

            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void RelayCommand_Execute_CallsDelegate()
        {
            bool executed = false;

            var command = new RelayCommand(
                _ => executed = true);

            command.Execute(null);

            Assert.True(executed);
        }

        [Fact]
        public void RelayCommand_RaiseCanExecuteChanged_RaisesEvent()
        {
            var command = new RelayCommand(_ => { });

            bool raised = false;

            command.CanExecuteChanged += (_, __) => raised = true;

            command.RaiseCanExecuteChanged();

            Assert.True(raised);
        }

        [Fact]
        public void RelayCommand_Constructor_NullExecute_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new RelayCommand(null));
        }
    }
}
