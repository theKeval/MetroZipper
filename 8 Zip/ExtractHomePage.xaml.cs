﻿using _8_Zip.Helper;
using SharpCompress.Reader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace _8_Zip
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ExtractHomePage : _8_Zip.Common.LayoutAwarePage
    {
        public ExtractHomePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        public static StorageFolder openedFolder;
        public static ObservableCollection<StorageFolder> previousFolder = new ObservableCollection<StorageFolder>();
        public ObservableCollection<FileFolderModel> parameterContent;
        //public ObservableCollection<FileFolderModel> parameterContent_1 = new ObservableCollection<FileFolderModel>();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                parameterContent = (ObservableCollection<FileFolderModel>) e.Parameter;
                gv_contentViewer.ItemsSource = parameterContent;

                openedFolder = HomePage.RootFolder;
            }
            catch (Exception ex)
            {
                MessageDialog ms = new MessageDialog(ex.Message);
                ms.ShowAsync();
            }
        }

        private async void gv_contentViewer_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (gv_contentViewer.SelectedItem != null)
                {
                    var selectedFileFolder = (FileFolderModel) gv_contentViewer.SelectedItem;
                    
                    if (selectedFileFolder.isFolder)
                    {
                        previousFolder = openedFolder;
                        openedFolder = await openedFolder.GetFolderAsync(selectedFileFolder.name);
                        parameterContent.Clear();

                        foreach (var item in await openedFolder.GetFoldersAsync())
                        {
                            parameterContent.Add(new FileFolderModel
                            {
                                name = item.Name,
                                isFolder = true
                            });
                        }

                        foreach (var item in await openedFolder.GetFilesAsync())
                        {
                            parameterContent.Add(new FileFolderModel
                            {
                                name = item.Name,
                                isFolder = false
                            });
                        }

                        gv_contentViewer.ItemsSource = parameterContent;

                    }
                    else
                    {

                    }
                }

            }
            catch (Exception ex)
            {
                MessageDialog ms = new MessageDialog(ex.Message);
                ms.ShowAsync();
            }
        }

        private async void Btn_ExtractAll_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                isBusy.IsBusy = true;

                var folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                folderPicker.FileTypeFilter.Add(".png");
                folderPicker.FileTypeFilter.Add(".jpg");
                folderPicker.FileTypeFilter.Add(".txt");
                folderPicker.FileTypeFilter.Add(".zip");
                folderPicker.FileTypeFilter.Add(".rar");
                StorageFolder xyz = await folderPicker.PickSingleFolderAsync();

                StorageFile abc = HomePage.abc;

                await extractCompressedFile_ReaderFactory(abc, xyz);

                isBusy.IsBusy = false;
            }
            catch (Exception ex)
            {
                isBusy.IsBusy = false;

                MessageDialog ms = new MessageDialog(ex.Message);
                ms.ShowAsync();
            }
            finally
            {
                isBusy.IsBusy = false;
            }
        }

        public StorageFolder selectedFolder;
        private async Task extractCompressedFile_ReaderFactory(StorageFile sourceCompressedFile, StorageFolder destinationFolder)      //, StorageFolder destinationFolder
        {
            using (Stream fileStream = await sourceCompressedFile.OpenStreamForReadAsync())
            {
                var Reader = ReaderFactory.Open(fileStream);
                StorageFolder folder = await destinationFolder.CreateFolderAsync(sourceCompressedFile.DisplayName, CreationCollisionOption.OpenIfExists);

                //foreach (var entry in archive.Entries)
                while (Reader.MoveToNextEntry())
                {
                    selectedFolder = folder;
                    if (!Reader.Entry.IsDirectory)
                    {
                        if (Reader.Entry.FilePath.Contains("/"))
                        {
                            string[] splitedPath = Reader.Entry.FilePath.Split('/');
                            string FileName = splitedPath[(splitedPath.Count() - 1)];

                            foreach (var item in splitedPath)
                            {
                                if (item == splitedPath.First() && !(item == splitedPath.Last()))
                                {
                                    StorageFolder sf = await selectedFolder.CreateFolderAsync(item, CreationCollisionOption.OpenIfExists);
                                    selectedFolder = sf;
                                }
                                else if (!(item == splitedPath.Last()))
                                {
                                    StorageFolder sf1 = await selectedFolder.CreateFolderAsync(item, CreationCollisionOption.OpenIfExists);
                                    selectedFolder = sf1;
                                    //StorageFolder sf1 = await sf.createfol
                                }
                                else if (item == splitedPath.Last())
                                {
                                    StorageFile file = await selectedFolder.CreateFileAsync(FileName, CreationCollisionOption.OpenIfExists);
                                    Stream newFileStream = await file.OpenStreamForWriteAsync();

                                    MemoryStream streamEntry = new MemoryStream();
                                    Reader.WriteEntryTo(streamEntry);
                                    //entry.WriteTo(streamEntry);
                                    // buffer for extraction data
                                    byte[] data = streamEntry.ToArray();

                                    newFileStream.Write(data, 0, data.Length);
                                    newFileStream.Flush();
                                    newFileStream.Dispose();
                                }
                            }
                        }
                        else
                        {
                            StorageFile file = await selectedFolder.CreateFileAsync(Reader.Entry.FilePath, CreationCollisionOption.OpenIfExists);
                            Stream newFileStream = await file.OpenStreamForWriteAsync();

                            MemoryStream streamEntry = new MemoryStream();
                            Reader.WriteEntryTo(streamEntry);
                            //entry.WriteTo(streamEntry);
                            // buffer for extraction data
                            byte[] data = streamEntry.ToArray();

                            newFileStream.Write(data, 0, data.Length);
                            newFileStream.Flush();
                            newFileStream.Dispose();
                        }
                        #region commented old code
                        //StorageFile newFile = await folder.CreateFileAsync(Reader.Entry.FilePath, CreationCollisionOption.FailIfExists);
                        //Stream newFileStream = await newFile.OpenStreamForWriteAsync();

                        //MemoryStream streamEntry = new MemoryStream();
                        //Reader.WriteEntryTo(streamEntry);
                        ////entry.WriteTo(streamEntry);
                        //// buffer for extraction data
                        //byte[] data = streamEntry.ToArray();

                        //newFileStream.Write(data, 0, data.Length);
                        //newFileStream.Flush();
                        //newFileStream.Dispose();
                        #endregion
                    }
                    else if (Reader.Entry.IsDirectory)
                    {
                        if (Reader.Entry.FilePath.Contains("/"))
                        {
                            string[] splitedPathWithBlank = Reader.Entry.FilePath.Split('/');
                            string FolderName = splitedPathWithBlank[splitedPathWithBlank.Length - 2];    // can also be written as splitedPath[splitedPath.count() - 2]
                            List<string> splitedPath = new List<string>();
                            List<string> splitedPathFinal = new List<string>();

                            foreach (var item in splitedPathWithBlank)
                            {
                                if (!(item == splitedPathWithBlank.Last()))
                                {
                                    splitedPath.Add(item);
                                }
                            }

                            var i = 1;
                            foreach (var item in splitedPath)
                            {
                                if (!(i == splitedPath.Count()))
                                {
                                    splitedPathFinal.Add(item);
                                    //item = item + ".last";
                                }
                                else
                                {
                                    splitedPathFinal.Add(item + ".last");
                                }
                                i++;
                            }

                            foreach (var item in splitedPathFinal)
                            {
                                if (item == splitedPathFinal.First() && item != splitedPathFinal.Last())
                                {
                                    StorageFolder sf = await selectedFolder.CreateFolderAsync(item, CreationCollisionOption.OpenIfExists);
                                    selectedFolder = sf;
                                }
                                else if (item != splitedPathFinal.Last())
                                {
                                    StorageFolder sf1 = await selectedFolder.CreateFolderAsync(item, CreationCollisionOption.OpenIfExists);
                                    selectedFolder = sf1;
                                }
                                else if (item == splitedPathFinal.Last())
                                {
                                    StorageFolder sfLast = await selectedFolder.CreateFolderAsync(FolderName, CreationCollisionOption.OpenIfExists);
                                }
                            }
                        }

                        //StorageFolder newFolder = await folder.CreateFolderAsync(Reader.Entry.FilePath, CreationCollisionOption.ReplaceExisting);
                    }
                }
            }
        }


        private async void BtnGoUp_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //var parentFolderPath = openedFolder.Path;

            openedFolder = previousFolder;
            parameterContent.Clear();

            foreach (var item in await previousFolder.GetFoldersAsync())
            {
                parameterContent.Add(new FileFolderModel
                {
                    name = item.Name,
                    isFolder = true
                });
            }

            foreach (var item in await openedFolder.GetFilesAsync())
            {
                parameterContent.Add(new FileFolderModel
                {
                    name = item.Name,
                    isFolder = false
                });
            }

            gv_contentViewer.ItemsSource = parameterContent;

        }

    }
}
