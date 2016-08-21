using System;
using ThinMvvm.DependencyInjection.Infrastructure;

namespace ThinMvvm.Windows
{
    public sealed class WindowsAppConfig
    {
        public TimeSpan SavedStateExpirationTime { get; }

        public bool IsSoftwareBackButtonEnabled { get; }

        public WindowsSplashScreenGraphics SplashScreenGraphics { get; }

        public IWindowsApplicationSkeleton Skeleton { get; }

        public Type SkeletonViewModelType { get; }

        public Func<ObjectCreator, WindowsAppCore> CoreFactory { get; }


        public WindowsAppConfig( TimeSpan savedStateExpirationTime,
                                 bool isSoftwareBackButtonEnabled,
                                 WindowsSplashScreenGraphics splashScreenGraphics,
                                 IWindowsApplicationSkeleton skeleton,
                                 Type skeletonViewModelType,
                                 Func<ObjectCreator, WindowsAppCore> coreFactory )
        {
            SavedStateExpirationTime = savedStateExpirationTime;
            IsSoftwareBackButtonEnabled = isSoftwareBackButtonEnabled;
            SplashScreenGraphics = splashScreenGraphics;
            Skeleton = skeleton;
            SkeletonViewModelType = skeletonViewModelType;
            CoreFactory = coreFactory;
        }
    }
}