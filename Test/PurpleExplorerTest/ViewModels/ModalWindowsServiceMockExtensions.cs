using Moq;
using PurpleExplorer.Helpers;
using PurpleExplorer.ViewModels;
using PurpleExplorer.Views;

namespace PurpleExplorerTest.ViewModels;

internal static class ModalWindowsServiceMockExtensions
{
    internal static Mock<IModalWindowService> Setup_ShowModalWindow(
        this Mock<IModalWindowService> me, 
        ConnectionStringWindowViewModel connectionStringWindowViewModel)
    {
        me.Setup(m =>
            m.ShowModalWindow<ConnectionStringWindow, ConnectionStringWindowViewModel>(It.IsAny<ConnectionStringWindowViewModel>()))
            .ReturnsAsync(connectionStringWindowViewModel);
        
        return me;
    }
}