using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MarkdownPreview
{
    public sealed partial class MainPage : Page
    {
        private ResourceLoader loader = new ResourceLoader();

        private bool aboutShown;
        private string about = MDParser.Parse(
@"# Usage

Use your Windows' **Open with** context menu on a markdown file to
open it with the viewer.

This application uses your profile-wide theme. You can temporarily
switch your theme. Please note that this change will _not_ be saved.

# License

## MarkdownPreview (This window and stuff)

Copyright 2018 Joel Fridolin Meyer

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
contributors may be used to endorse or promote products derived from
this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS &quot;AS IS&quot;
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

## CommonMark.NET (Markdown Parser)

Copyright (c) 2014, Kārlis Gaņģis
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

   * Redistributions of source code must retain the above copyright
     notice, this list of conditions and the following disclaimer.

   * Redistributions in binary form must reproduce the above copyright
     notice, this list of conditions and the following disclaimer in the
     documentation and/or other materials provided with the distribution.

   * Neither the name of Kārlis Gaņģis nor the names of other contributors 
     may be used to endorse or promote products derived from this software 
     without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS &quot;AS IS&quot; AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
");
        private string content;

        public MainPage()
        {
            this.InitializeComponent();

            string usage = loader.GetString("usage");
            string license = loader.GetString("license");
            about = MDParser.Parse(usage + "\n\n" + license);

            bool defaultDark = App.Current.RequestedTheme == ApplicationTheme.Dark;
            this.RequestedTheme = defaultDark ? ElementTheme.Dark : ElementTheme.Light;

            tsDark.IsOn = defaultDark;
            tsDark.Toggled += ChangeTheme;

            content = about;
            aboutShown = true;
            bDocument.Visibility = Visibility.Collapsed;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var args = e.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;

            if (args == null) return;
            if (args.Kind != Windows.ApplicationModel.Activation.ActivationKind.File) return;

            var fileArgs = args as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
            string filePath = fileArgs.Files[0].Path;
            var file = (StorageFile)fileArgs.Files[0];
            await LoadMarkdownFile(file);
            ApplicationView view = ApplicationView.GetForCurrentView();
            view.Title = file.Name;
        }

        private async Task LoadMarkdownFile(StorageFile file)
        {
            var read = await FileIO.ReadTextAsync(file);
            var html = MDParser.Parse(read);
            content = html;
            aboutShown = false;

            SetView(content);
        }

        private void OpenAbout(object sender, RoutedEventArgs e)
        {
            bAbout.Visibility = Visibility.Collapsed;
            bDocument.Visibility = Visibility.Visible;
            aboutShown = true;

            SetView(about);
        }

        private void CloseAbout(object sender, RoutedEventArgs e)
        {
            bAbout.Visibility = Visibility.Visible;
            bDocument.Visibility = Visibility.Collapsed;
            aboutShown = false;

            SetView(content);
        }

        private void ChangeTheme(object sender, RoutedEventArgs e)
        {
#if DEBUG
            Debug.WriteLine("Switch Theme");
#endif
            SetTheme(tsDark.IsOn ? Theme.Dark : Theme.Light);
        }

        private void SetView(string html)
        {
            string result = MDParser.GetHeader(IsDark());
            result += MDParser.Parse(html);
            result += MDParser.GetFooter();

#if DEBUG
            Debug.WriteLine(result);
#endif

            wvView.NavigateToString(result);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetView(content);
        }

        private void SetTheme(Theme theme)
        {
            switch (theme)
            {
                case Theme.Dark:
                    this.RequestedTheme = ElementTheme.Dark;
                    SetView(aboutShown ? about : content);
                    break;
                case Theme.Light:
                    this.RequestedTheme = ElementTheme.Light;
                    SetView(aboutShown ? about : content);
                    break;
            }
        }

        private bool IsDark()
        {
            return this.RequestedTheme == ElementTheme.Dark;
        }
    }

    public enum Theme
    {
        Dark,
        Light,
    }
}
