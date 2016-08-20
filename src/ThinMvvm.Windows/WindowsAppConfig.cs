using System;
using ThinMvvm.DependencyInjection.Infrastructure;

namespace ThinMvvm.Windows
{
    public sealed class WindowsAppConfig
    {
        public bool IsSoftwareBackButtonEnabled { get; }

        public WindowsSplashScreenGraphics SplashScreenGraphics { get; }

        public IWindowsApplicationSkeleton Skeleton { get; }

        public Type SkeletonViewModelType { get; }

        public Func<ObjectCreator, WindowsAppCore> CoreFactory { get; }


        public WindowsAppConfig( bool isSoftwareBackButtonEnabled,
                                 WindowsSplashScreenGraphics splashScreenGraphics,
                                 IWindowsApplicationSkeleton skeleton,
                                 Type skeletonViewModelType,
                                 Func<ObjectCreator, WindowsAppCore> coreFactory )
        {
            IsSoftwareBackButtonEnabled = isSoftwareBackButtonEnabled;
            SplashScreenGraphics = splashScreenGraphics;
            Skeleton = skeleton;
            SkeletonViewModelType = skeletonViewModelType;
            CoreFactory = coreFactory;
        }
    }
}