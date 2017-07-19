using CustomVisionAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace SampleAppUWP
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string customImageUrlEndpoint = "<< Custom Vision Service URL>>/url";
        private string customImageImgEndpoint = "<< Custom Vision Service URL>>/image";
        private string predictionKey = "<< Prediction-Key >>";

        public MainPage()
        {
            this.InitializeComponent();
            access = new CustomVisionAccess.CustomVisionAccess(predictionKey);
            predictionResults = new List<PredictionResult>();
        }

        private async void rbFile_Click(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb.Content.ToString() == "File")
            {
                rbURL.IsChecked = false;
            }
            else
            {
                bool isChanged = false;
                if (!string.IsNullOrEmpty(tbImgURL.Text))
                {
                    if (Uri.IsWellFormedUriString(tbImgURL.Text, UriKind.Absolute))
                    {
                        rbFile.IsChecked = false;
                        isChanged = true;
                    }
                }
                if (!isChanged)
                {
                    var dialog = new MessageDialog("Please specify valid image url!");
                    await dialog.ShowAsync();
                    rbURL.IsChecked = false;
                }
            }
        }

        private async void buttonFileSelect_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".jpg");
            var file = await picker.PickSingleFileAsync();
            selectedFile = file;
            tbSelectedFileName.Text = file.Path;
            rbFile.IsChecked = true;
            rbURL.IsChecked = false;
        }

        StorageFile selectedFile;
        CustomVisionAccess.CustomVisionAccess access;
        List<PredictionResult> predictionResults;

        private async void buttonAnalyze_Click(object sender, RoutedEventArgs e)
        {
            string result="";
            if (rbFile.IsChecked.Value && selectedFile!=null)
            {
                using (var stream = await selectedFile.OpenStreamForReadAsync())
                {
                    result = await access.AnalyzeAsync(customImageImgEndpoint, stream);
                    StoreResults(result, tbSelectedFileName.Text);
                }
            }
            else if (rbURL.IsChecked.Value)
            {
                result = await access.AnalyzeAsync(customImageUrlEndpoint, tbImgURL.Text);
                StoreResults(result, tbImgURL.Text);
            }
            tbResult.Text = result;
        }

        private void StoreResults(string result, string name)
        {
            var lists = access.ParseJSON(result);
            foreach (var l in lists)
            {
                l.Target = name;
            }
            predictionResults.Concat(lists);
        }

        private void tbImgURL_TextCompositionEnded(TextBox sender, TextCompositionEndedEventArgs args)
        {
            var tb = sender as TextBox;
            if (Uri.IsWellFormedUriString(tb.Text, UriKind.Absolute))
            {
                rbURL.IsChecked = true;
                rbFile.IsChecked = false;
            }

        }
    }
}
