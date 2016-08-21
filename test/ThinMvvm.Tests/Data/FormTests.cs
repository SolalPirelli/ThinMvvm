using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThinMvvm.Data;
using ThinMvvm.Data.Infrastructure;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests.Data
{
    public sealed class FormTests
    {
        private sealed class IntForm : Form<int>
        {
            public Func<Task<int>> InitialLoad;
            public Func<int, Task> Submit;


            public IntForm( Func<Task<int>> initialLoad, Func<int, Task> submit )
            {
                InitialLoad = initialLoad;
                Submit = submit;
            }


            protected override Task<int> LoadInitialInputAsync()
            {
                return InitialLoad();
            }

            protected override Task SubmitAsync( int input )
            {
                return Submit( input );
            }
        }

        [Fact]
        public void InitialState()
        {
            var form = new IntForm( () => Task.FromResult( 42 ), _ => TaskEx.CompletedTask );

            Assert.Equal( 0, form.Input );
            Assert.Equal( 0, ( (IForm) form ).Input );
            Assert.Equal( FormStatus.None, form.Status );
            Assert.Null( form.Error );
        }

        [Fact]
        public async Task InitializationInProgress()
        {
            var taskSource = new TaskCompletionSource<int>();
            var form = new IntForm( () => taskSource.Task, _ => TaskEx.CompletedTask );

            var hits = new List<string>();
            form.PropertyChanged += ( _, e ) =>
            {
                if( e.PropertyName == nameof( IntForm.Status ) && hits.Count == 0 )
                {
                    Assert.Equal( 0, form.Input );
                    Assert.Equal( 0, ( (IForm) form ).Input );
                    Assert.Equal( FormStatus.Initializing, form.Status );
                    Assert.Null( form.Error );
                }

                hits.Add( e.PropertyName );
            };

            var task = form.InitializeAsync();

            Assert.Equal( new[] { nameof( IntForm.Status ) }, hits );

            taskSource.SetResult( 0 );
            await task;
        }

        [Fact]
        public async Task SuccessfulInitialization()
        {
            var taskSource = new TaskCompletionSource<int>();
            var form = new IntForm( () => taskSource.Task, _ => TaskEx.CompletedTask );

            var task = form.InitializeAsync();

            var hits = new List<string>();
            form.PropertyChanged += ( _, e ) =>
            {
                hits.Add( e.PropertyName );

                if( e.PropertyName == nameof( IntForm.Status ) )
                {
                    Assert.Equal( 42, form.Input );
                    Assert.Equal( 42, ( (IForm) form ).Input );
                    Assert.Equal( FormStatus.Initialized, form.Status );
                    Assert.Null( form.Error );
                }
            };

            taskSource.SetResult( 42 );
            await task;

            Assert.Equal( new[] { nameof( IntForm.Input ), nameof( IntForm.Status ) }, hits );
        }

        [Fact]
        public async Task FailedInitialization()
        {
            var ex = new MyException();
            var taskSource = new TaskCompletionSource<int>();
            var form = new IntForm( () => taskSource.Task, _ => TaskEx.CompletedTask );

            var task = form.InitializeAsync();

            var hits = new List<string>();
            form.PropertyChanged += ( _, e ) =>
            {
                hits.Add( e.PropertyName );

                if( e.PropertyName == nameof( IntForm.Status ) )
                {
                    Assert.Equal( 0, form.Input );
                    Assert.Equal( 0, ( (IForm) form ).Input );
                    Assert.Equal( FormStatus.None, form.Status );
                    Assert.Equal( ex, form.Error );
                }
            };

            taskSource.SetException( ex );
            await task;

            Assert.Equal( new[] { nameof( IntForm.Error ), nameof( IntForm.Status ) }, hits );
        }

        [Fact]
        public async Task CannotInitializeWhileAlreadyInitializing()
        {
            var taskSource = new TaskCompletionSource<int>();
            var form = new IntForm( () => taskSource.Task, _ => TaskEx.CompletedTask );

            var task = form.InitializeAsync();

            await Assert.ThrowsAsync<InvalidOperationException>( form.InitializeAsync );

            taskSource.SetResult( 0 );
            await task;
        }

        [Fact]
        public async Task CannotInitializeTwice()
        {
            var form = new IntForm( () => Task.FromResult( 0 ), _ => TaskEx.CompletedTask );

            await form.InitializeAsync();

            await Assert.ThrowsAsync<InvalidOperationException>( form.InitializeAsync );
        }

        [Fact]
        public async Task CanRetryInitializationAfterFailure()
        {
            var task = TaskEx.FromException<int>( new Exception() );
            var form = new IntForm( () => task, _ => TaskEx.CompletedTask );

            await form.InitializeAsync();

            task = Task.FromResult( 42 );
            await form.InitializeAsync();

            Assert.Equal( 42, form.Input );
            Assert.Equal( 42, ( (IForm) form ).Input );
            Assert.Equal( FormStatus.Initialized, form.Status );
            Assert.Null( form.Error );
        }

        [Fact]
        public async Task CannotSubmitWithoutInitializing()
        {
            var form = new IntForm( () => Task.FromResult( 0 ), _ => TaskEx.CompletedTask );

            await Assert.ThrowsAsync<InvalidOperationException>( form.SubmitAsync );
        }

        [Fact]
        public async Task CannotSubmitDuringInitialization()
        {
            var taskSource = new TaskCompletionSource<int>();
            var form = new IntForm( () => taskSource.Task, _ => TaskEx.CompletedTask );

            var task = form.InitializeAsync();

            await Assert.ThrowsAsync<InvalidOperationException>( form.InitializeAsync );

            taskSource.SetResult( 0 );
            await task;
        }

        [Fact]
        public async Task CannotSubmitAfterFailedInitialization()
        {
            var form = new IntForm( () => TaskEx.FromException<int>( new MyException() ), _ => TaskEx.CompletedTask );

            await form.InitializeAsync();

            await Assert.ThrowsAsync<InvalidOperationException>( form.SubmitAsync );
        }

        [Fact]
        public async Task SubmithInProgress()
        {
            var taskSource = new TaskCompletionSource<int>();
            var form = new IntForm( () => Task.FromResult( 42 ), _ => taskSource.Task );

            await form.InitializeAsync();

            var hits = new List<string>();
            form.PropertyChanged += ( _, e ) =>
            {
                if( e.PropertyName == nameof( IntForm.Status ) && hits.Count == 0 )
                {
                    Assert.Equal( 42, form.Input );
                    Assert.Equal( 42, ( (IForm) form ).Input );
                    Assert.Equal( FormStatus.Submitting, form.Status );
                    Assert.Null( form.Error );
                }

                hits.Add( e.PropertyName );
            };

            var task = form.SubmitAsync();

            Assert.Equal( new[] { nameof( IntForm.Status ) }, hits );

            taskSource.SetResult( 0 );
            await task;
        }

        [Fact]
        public async Task SuccessfulSubmit()
        {
            var taskSource = new TaskCompletionSource<int>();
            var form = new IntForm( () => Task.FromResult( 42 ), _ => taskSource.Task );

            await form.InitializeAsync();

            var task = form.SubmitAsync();

            var hits = new List<string>();
            form.PropertyChanged += ( _, e ) =>
            {
                hits.Add( e.PropertyName );

                if( e.PropertyName == nameof( IntForm.Status ) )
                {
                    Assert.Equal( 42, form.Input );
                    Assert.Equal( 42, ( (IForm) form ).Input );
                    Assert.Equal( FormStatus.Submitted, form.Status );
                    Assert.Null( form.Error );
                }
            };

            taskSource.SetResult( 42 );
            await task;

            Assert.Equal( new[] { nameof( IntForm.Status ) }, hits );
        }

        [Fact]
        public async Task FailedSubmit()
        {
            var ex = new MyException();
            var taskSource = new TaskCompletionSource<int>();
            var form = new IntForm( () => Task.FromResult( 42 ), _ => taskSource.Task );

            await form.InitializeAsync();

            var task = form.SubmitAsync();

            var hits = new List<string>();
            form.PropertyChanged += ( _, e ) =>
            {
                hits.Add( e.PropertyName );

                if( e.PropertyName == nameof( IntForm.Status ) )
                {
                    Assert.Equal( 42, form.Input );
                    Assert.Equal( 42, ( (IForm) form ).Input );
                    Assert.Equal( FormStatus.Submitted, form.Status );
                    Assert.Equal( ex, form.Error );
                }
            };

            taskSource.SetException( ex );
            await task;

            Assert.Equal( new[] { nameof( IntForm.Error ), nameof( IntForm.Status ) }, hits );
        }

        [Fact]
        public async Task SubmitUsesInput()
        {
            var hit = false;
            var form = new IntForm( () => Task.FromResult( 42 ), v =>
            {
                hit = true;
                Assert.Equal( 42, v );
                return TaskEx.CompletedTask;
            } );

            await form.InitializeAsync();
            await form.SubmitAsync();

            Assert.True( hit );
        }
    }
}