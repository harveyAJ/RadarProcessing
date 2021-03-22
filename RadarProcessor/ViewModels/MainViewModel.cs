using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using RadarProcessor.Enums;
using RadarProcessor.Models;
using RadarProcessor.Services;

namespace RadarProcessor.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly RdxFileReader rdxFileReader;
        private readonly StatusViewModel statusViewModel;
        private readonly IDialogService dialogService;

        public MainViewModel(
            StatusViewModel statusVm,
            IDialogService dialogService)
        {
            this.statusViewModel = statusVm;
            this.rdxFileReader = new RdxFileReader();
            this.dialogService = dialogService;
            this.Initialise();

            this.plotModel = new PlotModel();
            this.plotModel.Axes.Add(this.xAxis);
            this.plotModel.Axes.Add(this.yAxis);
            this.plotModel.PlotType = PlotType.Cartesian;

            this.rdxFileReader.PropertyChanged += this.OnRdxFileReaderPropertyChanged;
        }

        private void Initialise()
        {
            this.IsRdxSelected = false;
            this.IsOverflight = false;
            this.IsArrival = true;
            this.IsDeparture = true;
            if (this.cts == null)
            {
                this.cts = new CancellationTokenSource();
                return;
            }
            this.cts.Dispose();
            this.cts = new CancellationTokenSource();
        }

        private void OnRdxFileReaderPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == "Status")
            {
                this.Status = this.rdxFileReader.Status;
            }

            if (eventArgs.PropertyName == "FromDateTime")
            {
                this.FromDateTime = this.rdxFileReader.FromDateTime;
            }

            if (eventArgs.PropertyName == "ToDateTime")
            {
                this.ToDateTime = this.rdxFileReader.ToDateTime;
            }
        }


        #region PlotModel

        private PlotModel plotModel;
        private readonly LinearAxis xAxis = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "X (m)"
        };
        private readonly LinearAxis yAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Y (m)"
        };

        public PlotModel PlotModel
        {
            get => this.plotModel;
            set
            {
                this.plotModel = value;
                this.RaisePropertyChanged();
            }
        }

        private void ResetAxes()
        {
            //This is the simplest way I found to zoom around the plotting area
            this.PlotModel.Axes[0].Reset();
            this.PlotModel.Axes[1].Reset();
        }

        #endregion

        #region Filtering

        private bool isArrival = true;
        private bool isDeparture = true;
        private bool isOverflight;
        private DateTime fromDateTime;
        private DateTime toDateTime;

        public DateTime FromDateTime //{ get; set; } //= new DateTime(2017, 06, 21);
        {
            get => this.fromDateTime;
            set
            {
                this.fromDateTime = value;
                this.RaisePropertyChanged();
            }
        }

        public DateTime ToDateTime //{ get; set; } //= new DateTime(2017, 06, 22);
        {
            get => this.toDateTime;
            set
            {
                this.toDateTime = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsArrival
        {
            get => this.isArrival;
            set
            {
                this.isArrival = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsDeparture
        {
            get => this.isDeparture;
            set
            {
                this.isDeparture = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsOverflight
        {
            get => this.isOverflight;
            set
            {
                this.isOverflight = value;
                this.RaisePropertyChanged();
            }
        }

        #endregion

        #region Track data manipulation
        private CancellationTokenSource cts;

        private RelayCommand browseRdxCommand;
        private RelayCommand cancelCommand;
        private RelayCommand loadRdxCommand;

        private string selectedRdxPath;
        private bool isRdxSelected;

        public RelayCommand BrowseRdxCommand => this.browseRdxCommand ?? (this.browseRdxCommand = new RelayCommand(this.BrowseRdx));

        public RelayCommand CancelCommand => this.cancelCommand ?? (this.cancelCommand = new RelayCommand(this.Cancel));

        public RelayCommand LoadRdxCommand  => this.loadRdxCommand ?? (this.loadRdxCommand = new RelayCommand(this.LoadRdx));

        public string SelectedRdxPath
        {
            get => this.selectedRdxPath;
            set
            {
                this.selectedRdxPath = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsRdxSelected
        {
            get => this.isRdxSelected;
            set
            {
                this.isRdxSelected = value;
                this.RaisePropertyChanged();
            }
        }

        private async void BrowseRdx()
        {
            this.Status = $"Loading {Path.GetFileName(this.selectedRdxPath)}";
            this.isRdxSelected = false;
            this.SelectedRdxPath = this.dialogService.BrowseFile("RDX files (*.rdx)|*.rdx");
            this.rdxFileReader.RdxFilePath = this.SelectedRdxPath;
            var result = await this.rdxFileReader.Initialise(cts.Token);

            if (result != 0)
            {
                this.dialogService.ShowError("An error occured");
                return;
            }
            this.IsRdxSelected = true;
            this.Status = $"Data loaded";
            //TODO: maybe some verification? A sort of lazy loading?
        }

        private async void LoadRdx()
        {
            if (string.IsNullOrEmpty(this.SelectedRdxPath))
            {
                return;
            }

            try
            {
                var operationsAllowed = new List<OperationType>();
                if (this.IsOverflight)
                {
                    operationsAllowed.Add(OperationType.Overflight);
                }

                if (this.isArrival)
                {
                    operationsAllowed.Add(OperationType.Arrival);
                }

                if (this.IsDeparture)
                {
                    operationsAllowed.Add(OperationType.Departure);
                }

                this.rdxFileReader.RdxFilePath = this.SelectedRdxPath;
                this.rdxFileReader.FromDateTime = this.FromDateTime;
                this.rdxFileReader.ToDateTime = this.ToDateTime;
                this.rdxFileReader.OperationTypes = operationsAllowed.ToArray();
                var tracks = await this.rdxFileReader.ReadAsync(cts.Token);

                this.PlotModel.Series.Clear();
                foreach (var track in tracks)
                {
                    var lineSeries = new LineSeries
                    {
                        LineStyle = LineStyle.Solid,
                        MarkerType = MarkerType.None,
                        Color = OxyColors.Black,
                        StrokeThickness = 1
                    };
                    foreach (var point in track.TrackPoints.Select(p => new DataPoint(p.Xmetres, p.Ymetres)))
                    {
                        lineSeries.Points.Add(point);
                    }
                    this.PlotModel.Series.Add(lineSeries);
                }

                this.ResetAxes();
                this.PlotModel.InvalidatePlot(true);
            }
            catch (Exception e)
            {
                this.dialogService.ShowError(e.Message);
            }
        }

        private void Cancel()
        {
            this.cts.Cancel();
        }
        #endregion

        #region Status bar

        private string status;

        public string Status
        {
            get => this.status;
            set
            {
                this.status = value;
                this.RaisePropertyChanged();
            }
        }

        #endregion
    }
}