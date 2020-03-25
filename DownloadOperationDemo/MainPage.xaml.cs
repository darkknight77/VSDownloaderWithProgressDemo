using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DownloadOperationDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DownloadOperation downloadOperation;
        CancellationTokenSource cancellationToken;
        public MainPage()
        {
            this.InitializeComponent();
           
        }
         async private void download() {
           
            StorageFile file = await KnownFolders.VideosLibrary.CreateFileAsync("file.mp4", CreationCollisionOption.GenerateUniqueName);
            IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
            var downloader = new BackgroundDownloader();
                downloadOperation = downloader.CreateDownload(new Uri("https://mediaplatstorage1.blob.core.windows.net/windows-universal-samples-media/elephantsdream-clip-h264_sd-aac_eng-aac_spa-aac_eng_commentary-srt_eng-srt_por-srt_swe.mkv"), file);
                MediaSource mediaSource =
                      MediaSource.CreateFromDownloadOperation(downloadOperation);
            downloadOperation.IsRandomAccessRequired = true;
            var startAsyncTask = downloadOperation.StartAsync().AsTask();
            
            mediaPlayerElement.Source = mediaSource;
        }
        async private void downloadWithProgress()
        {

            StorageFile file = await KnownFolders.VideosLibrary.CreateFileAsync("file.mp4", CreationCollisionOption.GenerateUniqueName);
            IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
            var downloader = new BackgroundDownloader();
            downloadOperation = downloader.CreateDownload(new Uri("https://mediaplatstorage1.blob.core.windows.net/windows-universal-samples-media/elephantsdream-clip-h264_sd-aac_eng-aac_spa-aac_eng_commentary-srt_eng-srt_por-srt_swe.mkv"), file);
            Progress<DownloadOperation> progress = new Progress<DownloadOperation>(progressChanged);
            cancellationToken = new CancellationTokenSource();
            try
            {
                StatusText.Text = "Initializing...";
                downloadOperation.IsRandomAccessRequired = true;
                await downloadOperation.StartAsync().AsTask(cancellationToken.Token, progress);
                
            }
            catch (TaskCanceledException)
            {
                await downloadOperation.ResultFile.DeleteAsync();
                downloadOperation = null;
            }

            
        }

        private void progressChanged(DownloadOperation downloadOperation)
        {
            int progress = (int)(100 * ((double)downloadOperation.Progress.BytesReceived / (double)downloadOperation.Progress.TotalBytesToReceive));
            StatusText.Text = String.Format("{0} of {1} kb. downloaded - {2}% complete.", downloadOperation.Progress.BytesReceived / 1024, downloadOperation.Progress.TotalBytesToReceive / 1024, progress);
            switch (downloadOperation.Progress.Status)
            {
                case BackgroundTransferStatus.Running:
                    {
                        Debug.WriteLine("Downloading");
                        break;
                    }
                case BackgroundTransferStatus.PausedByApplication:
                    {
                        StatusText.Text = "Paused by user";
                        break;
                    }
                case BackgroundTransferStatus.PausedCostedNetwork:
                    {
                        break;
                    }
                case BackgroundTransferStatus.PausedNoNetwork:
                    {
                        StatusText.Text = "Paused because of no network";
                        break;
                    }
                case BackgroundTransferStatus.Error:
                    {
                        StatusText.Text = "An error occured while downloading.";
                        break;
                    }
            }
            if (progress >= 100)
            {
                Debug.WriteLine("Download complete Starting playback");

                MediaSource mediaSource = MediaSource.CreateFromDownloadOperation(downloadOperation);
                mediaPlayerElement.Source = mediaSource;
                mediaPlayerElement.AutoPlay = true;
            }
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            downloadWithProgress();
        }

        private void btn_pause_Click(object sender, RoutedEventArgs e)
        {
            downloadOperation.Pause();
        }

        private void btn_resume_Click(object sender, RoutedEventArgs e)
        {
            downloadOperation.Resume();
        }
    }
    }

