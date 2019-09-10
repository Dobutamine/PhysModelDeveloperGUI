using PhysModelLibrary;
using PhysModelLibrary.BaseClasses;
using PhysModelLibrary.Compartments;
using PhysModelLibrary.Connectors;
using PhysModelLibrary.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace PhysModelDeveloperGUI
{

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        PhysModelLibrary.Model currentModel = new PhysModelLibrary.Model();

        readonly DispatcherTimer updateTimer = new DispatcherTimer(DispatcherPriority.Render);
        int slowUpdater = 0;
        int graphicsRefreshInterval = 15;

        private bool diagramVisible = false;

        public bool DiagramVisible
        {
            get { return diagramVisible; }
            set { diagramVisible = value; OnPropertyChanged(); }
        }

        private bool monitorVisible = false;

        public bool MonitorVisible
        {
            get { return monitorVisible; }
            set { monitorVisible = value; OnPropertyChanged(); }
        }

        private bool trendVitalsVisible;

        public bool TrendVitalsVisible
        {
            get { return trendVitalsVisible; }
            set { trendVitalsVisible = value; OnPropertyChanged(); }
        }

        private bool trendBloodgasVisible;

        public bool TrendBloodgasVisible
        {
            get { return trendBloodgasVisible; }
            set { trendBloodgasVisible = value; OnPropertyChanged(); }
        }

        private bool pVLoopVisible;

        public bool PVLoopVisible
        {
            get { return pVLoopVisible; }
            set { pVLoopVisible = value; OnPropertyChanged(); }
        }

        PatientMonitor GraphPatientMonitor { get; set; }
        LoopGraph GraphPVLoop { get; set; }
        ModelDiagram GraphModelDiagram { get; set; }
        FastScrollingGraph GraphECG { get; set; }
        FastScrollingGraph GraphABP { get; set; }
        FastScrollingGraph GraphSPO2 { get; set; }
        FastScrollingGraph GraphETCO2 { get; set; }
        FastScrollingGraph GraphRESP { get; set; }

        TimeBasedGraph TrendGraph { get; set; }
        TimeBasedGraph BloodgasGraph { get; set; }

        public RelayCommand ChangeDrugEffectCommand { get; set; }
        public RelayCommand ChangeDrugCommand { get; set; }
        public RelayCommand ChangeBloodCompartmentCommand { get; set; }
        public RelayCommand ChangeRhythmCommand { get; set; }
        public RelayCommand ChangeGasCompartmentCommand { get; set; }
        public RelayCommand ChangeConnectorCommand { get; set; }
        public RelayCommand ChangeGexUnitCommand { get; set; }
        public RelayCommand ChangeContainerCommand { get; set; }
        public RelayCommand SaveModelStateCommand { get; set; }
        public RelayCommand LoadModelStateCommand { get; set; }
        public RelayCommand NewModelCommand { get; set; }
        public RelayCommand ListBoxUpdatedCommand { get; set; }
        public RelayCommand ExitCommand { get; set; }

        public RelayCommand RemoveBlood { get; set; }
        public RelayCommand AddDrugCommand { get; set; }

        DrugEffect selectedDrugEffect { get; set; }
        Drug selectedDrug { get; set; }
        BloodCompartment selectedBloodCompartment { get; set; }
        GasCompartment selectedGasCompartment { get; set; }
        Connector selectedConnector { get; set; }
        GasExchangeBlock selectedGex { get; set; }
        ContainerCompartment selectedContainer { get; set; }

        public MainWindowViewModel()
        {      
            currentModel.Initialize();
            currentModel.modelInterface.PropertyChanged += ModelInterface_PropertyChanged;
            currentModel.Start();
            

            updateTimer.Tick += UpdateTimer_Tick; ; ;
            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, graphicsRefreshInterval);
            updateTimer.Start();

            SetCommands();
            ConstructComponentLists();

            currentModel.analyzer.SelectBloodCompartment(currentModel.modelState.LV);

        }

        void SetCommands()
        {
            ChangeBloodCompartmentCommand = new RelayCommand(ChangeSelectedBloodCompartment);
            ChangeGasCompartmentCommand = new RelayCommand(ChangeSelectedGasCompartment);
            ChangeConnectorCommand = new RelayCommand(ChangeSelectedConnector);
            ChangeGexUnitCommand = new RelayCommand(ChangeSelectedGex);
            ChangeContainerCommand = new RelayCommand(ChangeSelectedContainer);
            SaveModelStateCommand = new RelayCommand(SaveModelState);
            LoadModelStateCommand = new RelayCommand(LoadModelState);
            NewModelCommand = new RelayCommand(NewModel);
            ExitCommand = new RelayCommand(ExitProgram);
            ChangeRhythmCommand = new RelayCommand(ChangeRhythm);
            AddDrugCommand = new RelayCommand(AddDrug);
            RemoveBlood = new RelayCommand(RemoveBloodVolume);
            ChangeDrugCommand = new RelayCommand(ChangeSelectedDrug);
            ChangeDrugEffectCommand = new RelayCommand(ChangeSelectedDrugEffect);

        }
   
        void RemoveBloodVolume(object p)
        {
            currentModel.modelInterface.AdjustWeight(1);
            //currentModel.modelInterface.AddBloodVolume(10);
        }
        void ChangeSelectedDrug(object p)
        {
            selectedDrug = (Drug)p;
            if (selectedDrug != null)
            {
                DrugDose = selectedDrug.Dose;
                DrugMetabolicRate = selectedDrug.MetabolicRate;
                DrugRenalClearanceRate = selectedDrug.RenalClearanceRate;
                DrugHepaticClearanceRate = selectedDrug.HepaticClearanceRate;
                drugEffects.Clear();
                foreach(DrugEffect d in selectedDrug.DrugEffects)
                {
                    drugEffects.Add(d);
                }
         

            }
        }

        void ChangeSelectedDrugEffect(object p)
        {
            selectedDrugEffect = (DrugEffect)p;
            if (selectedDrugEffect != null)
            {
                DrugEffectDoseDependent = selectedDrugEffect.DoseDependent;
                DrugEffectGain = selectedDrugEffect.Gain;
                DrugEffectTimeConstant = selectedDrugEffect.TimeConstant;
                DrugEffectSaturation = selectedDrugEffect.EffectConcentrationSaturation;
                DrugEffectThreshold = selectedDrugEffect.EffectConcentrationThreshold;
                DrugEffectSite = selectedDrugEffect.EffectSite;

            }
        }
        void AddDrug(object p)
        {

        if (selectedDrug != null)
            {
                currentModel.drugModel.AddNewActiveDrug(selectedDrug);
            }

        }
        void ExitProgram(object p)
        {
            App.Current.Shutdown();
        }
        void ConstructComponentLists()
        {
            // first clear the lists
            bloodcompartments.Clear();
            gascompartments.Clear();
            connectors.Clear();
            gasexchangeUnits.Clear();
            containers.Clear();

            foreach (Drug d in currentModel.modelState.AvailableDrugs)
            {
                availableDrugs.Add(d);
            }

            foreach (BloodCompartment c in currentModel.modelState.bloodCompartments)
            {
                bloodcompartments.Add(c);
            }
            foreach (GasCompartment c in currentModel.modelState.gasCompartments)
            {
                gascompartments.Add(c);
            }
            foreach (Connector c in currentModel.modelState.bloodCompartmentConnectors)
            {
                connectors.Add(c);
            }
            foreach (Connector c in currentModel.modelState.gasCompartmentConnectors)
            {
                connectors.Add(c);
            }
            foreach (Connector c in currentModel.modelState.valveConnectors)
            {
                connectors.Add(c);
            }
            foreach(GasExchangeBlock c in currentModel.modelState.gasExchangeBlocks)
            {
                gasexchangeUnits.Add(c);
            }
            foreach (ContainerCompartment c in currentModel.modelState.containerCompartments)
            {
                containers.Add(c);
            }

            rhythmTypes.Add("SINUS");
            rhythmTypes.Add("PAC");
            rhythmTypes.Add("PVC");
            rhythmTypes.Add("AVBLOCK1");
            rhythmTypes.Add("AVBLOCK2a");
            rhythmTypes.Add("AVBLOCK2b");
            rhythmTypes.Add("AVBLOCK Complete");
            rhythmTypes.Add("VTOUTPUT");
            rhythmTypes.Add("VF");
            rhythmTypes.Add("LONGQT");
            rhythmTypes.Add("WPW");
            rhythmTypes.Add("SVT");
        }
       
        void ChangeRhythm(object p)
        {
            int selection = (int)p;
            currentModel.ecg.ChangeRhythm(selection);
        }
        void SaveModelState(object p)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "modelState";
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML files (.xml)|*.xml";

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                currentModel.modelInterface.SaveModelState(dlg.FileName);
            }
        }
        void LoadModelState(object p)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; 
            dlg.DefaultExt = ".xml"; 
            dlg.Filter = "XML files (.xml)|*.xml";

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                selectedBloodCompartment = null;
                selectedConnector = null;
                selectedContainer = null;
                selectedGasCompartment = null;
                selectedGex = null;

                // Open document
                currentModel.modelInterface.LoadModelState(dlg.FileName);
                currentModel.Start();
                ConstructComponentLists();
                BuildModelDiagram();
            }
        }
        void NewModel(object p)
        {
            currentModel.modelInterface.LoadDefaultModel();
            currentModel.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (DiagramVisible)
            {
                GraphModelDiagram.UpdatedMainDiagram();
            }
                  
            if (TrendVitalsVisible) TrendGraph.DrawData();
            if (trendBloodgasVisible) BloodgasGraph.DrawData();

            if (slowUpdater > 1000)
            {
                slowUpdater = 0;
                Heartrate = currentModel.modelInterface.HeartRate.ToString();
                Spo2 = currentModel.modelInterface.PulseOximeterOutput.ToString();
                Abp = currentModel.modelInterface.ArterialBloodPressure;
                Pap = currentModel.modelInterface.PulmonaryArteryPressure.ToString();
                Cvp = currentModel.modelInterface.CentralVenousPressure.ToString();
                Resprate = currentModel.modelInterface.RespiratoryRate.ToString();
                Temp = currentModel.modelInterface.PatientTemperature.ToString();
                Lvo = currentModel.modelInterface.LeftVentricularOutput.ToString();
                Rvo = currentModel.modelInterface.RightVentricularOutput.ToString();
                Ivcflow = currentModel.modelInterface.InferiorVenaCavaFlow.ToString();
                Svcflow = currentModel.modelInterface.SuperiorVenaCavaFlow.ToString();
                Myoflow = currentModel.modelInterface.CoronaryFlow.ToString();
                Lvstroke = currentModel.modelInterface.StrokeVolumeLeftVentricle.ToString();
                Rvstroke = currentModel.modelInterface.StrokeVolumeRightVentricle.ToString();
                Rapressures = currentModel.modelInterface.RightAtrialPressures;
                Lapressures = currentModel.modelInterface.LeftAtrialPressures;
                Rvpressures = currentModel.modelInterface.RightVentricularPressures;
                Lvpressures = currentModel.modelInterface.LeftVentricularPressures;
                Ravolumes = currentModel.modelInterface.RightAtrialVolumes;
                Lavolumes = currentModel.modelInterface.LeftAtrialVolumes;
                Rvvolumes = currentModel.modelInterface.RightVentricularVolumes;
                Lvvolumes = currentModel.modelInterface.LeftVentricularVolumes;
                Pdaflow = Math.Round(currentModel.modelInterface.PDAFlow, 1).ToString();
                MyoO2Index = Math.Round(currentModel.modelInterface.Mii, 3).ToString();
                Myocardialdo2 = currentModel.modelInterface.MyoO2Delivery.ToString();
                Braindo2 = Math.Round(currentModel.modelInterface.BrainO2Delivery, 1).ToString();
                Kidneysflow = currentModel.modelInterface.KidneysFlow.ToString();
                Liverflow = currentModel.modelInterface.LiverFlow.ToString();
                Brainflow = currentModel.modelInterface.BrainFlow.ToString();

                VTRef = Math.Round(currentModel.modelState.VERef, 0).ToString();
                VTMax = Math.Round(currentModel.modelState.VEMax, 0).ToString();
                Tidalvolume = currentModel.modelInterface.TidalVolume.ToString();
                TidalvolumeTarget = currentModel.modelInterface.TidalVolumeTarget.ToString();
                Minutevolume = currentModel.modelInterface.MinuteVolume.ToString();
                MinutevolumeTarget = currentModel.modelInterface.MinuteVolumeTarget.ToString();
                Alveolarvolume = currentModel.modelInterface.AlveolarVolume;
                //TotalVolume = currentModel.modelInterface.TotalBloodVolume().ToString();

                Appliedpressure = currentModel.modelInterface.AppliedAirwayPressure;
                Airwaypressure = currentModel.modelInterface.AirwayPressure;
                Alvleftpressure = currentModel.modelInterface.AlveolarLeftPressure;
                Alvrightpressure = currentModel.modelInterface.AlveolarRightPressure;

                Ph = currentModel.modelInterface.ArterialPH.ToString();
                Pao2 = currentModel.modelInterface.ArterialPO2.ToString();
                Paco2 = currentModel.modelInterface.ArterialPCO2.ToString();
                Hco3 = currentModel.modelInterface.ArterialHCO3.ToString();
                Be = currentModel.modelInterface.ArterialBE.ToString();

                Po2alv = currentModel.modelInterface.AlveolarPO2;
                Pco2alv = currentModel.modelInterface.AlveolarPCO2;
                LactateAA = Math.Round(currentModel.modelState.AA.Lact, 1).ToString();
                LactateUB = Math.Round(currentModel.modelState.UB.Lact, 1).ToString();
                LactateLB = Math.Round(currentModel.modelState.LB.Lact, 1).ToString();
                LactateBRAIN = Math.Round(currentModel.modelState.BRAIN.Lact, 1).ToString();
                LactateLIVER = Math.Round(currentModel.modelState.LIVER.Lact, 1).ToString();

                Drug1Concentration = currentModel.modelInterface.Drug1Concentration;
                Drug2Concentration = currentModel.modelInterface.Drug2Concentration;
                Drug3Concentration = currentModel.modelInterface.Drug3Concentration;
                Drug4Concentration = currentModel.modelInterface.Drug4Concentration;
                Drug5Concentration = currentModel.modelInterface.Drug5Concentration;
                Drug6Concentration = currentModel.modelInterface.Drug6Concentration;
                Drug7Concentration = currentModel.modelInterface.Drug7Concentration;
                Drug8Concentration = currentModel.modelInterface.Drug8Concentration;
                Drug9Concentration = currentModel.modelInterface.Drug9Concentration;
                Drug10Concentration = currentModel.modelInterface.Drug10Concentration;

                Endtidalco2 = currentModel.modelInterface.EndTidalCO2.ToString();

                if (MonitorVisible)
                {
                    GraphPatientMonitor.UpdateParameters(currentModel.modelInterface.HeartRate.ToString(),                                                       
                                                         currentModel.modelInterface.PulseOximeterOutput.ToString(),
                                                         currentModel.modelInterface.ArterialBloodPressure,
                                                         currentModel.modelInterface.EndTidalCO2.ToString(),
                                                         currentModel.modelInterface.RespiratoryRate.ToString());
                }
                UpdateTrendGraph();
                UpdateBloodgasGraph();


                Console.WriteLine(currentModel.modelInterface.TotalBloodVolume());

                GraphPVLoop.Draw();
            }

            slowUpdater += graphicsRefreshInterval;
        }

        private void ModelInterface_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ModelUpdated")
            {
                UpdatePatientMonitor();
                UpdatePVLoopGraph();

                
            }
            if (e.PropertyName ==  "StatusMessage")
            {
                ModelLog.Add(DateTime.Now + currentModel.modelInterface.StatusMessage);
            }
        }


        #region "dependent model variable getters"
        string _modelName = "";
        public string ModelName { get { return _modelName; } set { _modelName = value; OnPropertyChanged(); } }

        string _heartrate = "140";
        public string Heartrate { get { return _heartrate; } set { _heartrate = value; OnPropertyChanged(); } }

        string _spo2 = "99";
        public string Spo2 { get { return _spo2; } set { _spo2 = value; OnPropertyChanged(); } }

        string _abp = "99";
        public string Abp { get { return _abp; } set { _abp = value; OnPropertyChanged(); } }

        string _pap = "99";
        public string Pap { get { return _pap; } set { _pap = value; OnPropertyChanged(); } }

        string _cvp = "99";
        public string Cvp { get { return _cvp; } set { _cvp = value; OnPropertyChanged(); } }

        string _resprate = "99";
        public string Resprate { get { return _resprate; } set { _resprate = value; OnPropertyChanged(); } }

        string _temp = "99";
        public string Temp { get { return _temp; } set { _temp = value; OnPropertyChanged(); } }

        string _lvo = "99";
        public string Lvo { get { return _lvo; } set { _lvo = value; OnPropertyChanged(); } }

        string _rvo = "99";
        public string Rvo { get { return _rvo; } set { _rvo = value; OnPropertyChanged(); } }

        string _ivcflow = "99";
        public string Ivcflow { get { return _ivcflow; } set { _ivcflow = value; OnPropertyChanged(); } }

        string _svcflow = "99";
        public string Svcflow { get { return _svcflow; } set { _svcflow = value; OnPropertyChanged(); } }

        string _lvstroke = "99";
        public string Lvstroke { get { return _lvstroke; } set { _lvstroke = value; OnPropertyChanged(); } }

        string _rvstroke = "99";
        public string Rvstroke { get { return _rvstroke; } set { _rvstroke = value; OnPropertyChanged(); } }

        string _rapressures = "99";
        public string Rapressures { get { return _rapressures; } set { _rapressures = value; OnPropertyChanged(); } }

        string _lapressures = "99";
        public string Lapressures { get { return _lapressures; } set { _lapressures = value; OnPropertyChanged(); } }

        string _rvpressures = "99";
        public string Rvpressures { get { return _rvpressures; } set { _rvpressures = value; OnPropertyChanged(); } }

        string _lvpressures = "99";
        public string Lvpressures { get { return _lvpressures; } set { _lvpressures = value; OnPropertyChanged(); } }

        string _ravolumes = "99";
        public string Ravolumes { get { return _ravolumes; } set { _ravolumes = value; OnPropertyChanged(); } }

        string _lavolumes = "99";
        public string Lavolumes { get { return _lavolumes; } set { _lavolumes = value; OnPropertyChanged(); } }

        string _rvvolumes = "99";
        public string Rvvolumes { get { return _rvvolumes; } set { _rvvolumes = value; OnPropertyChanged(); } }

        string _lvvolumes = "99";
        public string Lvvolumes { get { return _lvvolumes; } set { _lvvolumes = value; OnPropertyChanged(); } }

        string _totalVolume = "99";
        public string TotalVolume { get { return _totalVolume; } set { _totalVolume = value; OnPropertyChanged(); } }

        string _myoflow = "99";
        public string Myoflow { get { return _myoflow; } set { _myoflow = value; OnPropertyChanged(); } }

        string _myoO2Index = "-";
        public string MyoO2Index { get { return _myoO2Index; } set { _myoO2Index = value; OnPropertyChanged(); } }

        string _pdaflow = "99";
        public string Pdaflow { get { return _pdaflow; } set { _pdaflow = value; OnPropertyChanged(); } }

        string _brainflow = "-";
        public string Brainflow { get { return _brainflow; } set { _brainflow = value; OnPropertyChanged(); } }

        string _kidneysflow = "-";
        public string Kidneysflow { get { return _kidneysflow; } set { _kidneysflow = value; OnPropertyChanged(); } }

        string _liverflow = "-";
        public string Liverflow { get { return _liverflow; } set { _liverflow = value; OnPropertyChanged(); } }

        string _intestinesflow = "-";
        public string Intestinesflow { get { return _intestinesflow; } set { _intestinesflow = value; OnPropertyChanged(); } }

        string _globaldo2 = "-";
        public string Globaldo2 { get { return _globaldo2; } set { _globaldo2 = value; OnPropertyChanged(); } }

        string _myocardialdo2 = "-";
        public string Myocardialdo2 { get { return _myocardialdo2; } set { _myocardialdo2 = value; OnPropertyChanged(); } }

        string _braindo2 = "-";
        public string Braindo2 { get { return _braindo2; } set { _braindo2 = value; OnPropertyChanged(); } }

        string _vtmax = "-";
        public string VTMax { get { return _vtmax; } set { _vtmax = value; OnPropertyChanged(); } }

        string _vtref = "-";
        public string VTRef { get { return _vtref; } set { _vtref = value; OnPropertyChanged(); } }

        string _tidalvolume = "-";
        public string Tidalvolume { get { return _tidalvolume; } set { _tidalvolume = value; OnPropertyChanged(); } }

        string _tidalvolumeTarget = "-";
        public string TidalvolumeTarget { get { return _tidalvolumeTarget; } set { _tidalvolumeTarget = value; OnPropertyChanged(); } }

        string _minutevolume = "-";
        public string Minutevolume { get { return _minutevolume; } set { _minutevolume = value; OnPropertyChanged(); } }

        string _minutevolumeTarget = "-";
        public string MinutevolumeTarget { get { return _minutevolumeTarget; } set { _minutevolumeTarget = value; OnPropertyChanged(); } }

        string _alveolarvolume = "-";
        public string Alveolarvolume { get { return _alveolarvolume; } set { _alveolarvolume = value; OnPropertyChanged(); } }

        string _appliedpressure = "-";
        public string Appliedpressure { get { return _appliedpressure; } set { _appliedpressure = value; OnPropertyChanged(); } }

        string _airwaypressure = "-";
        public string Airwaypressure { get { return _airwaypressure; } set { _airwaypressure = value; OnPropertyChanged(); } }

        string _alvleftpressure = "-";
        public string Alvleftpressure { get { return _alvleftpressure; } set { _alvleftpressure = value; OnPropertyChanged(); } }

        string _alvrightpressure = "-";
        public string Alvrightpressure { get { return _alvrightpressure; } set { _alvrightpressure = value; OnPropertyChanged(); } }

        string _ph = "-";
        public string Ph { get { return _ph; } set { _ph = value; OnPropertyChanged(); } }

        string _pao2 = "-";
        public string Pao2 { get { return _pao2; } set { _pao2 = value; OnPropertyChanged(); } }
        string _paco2 = "-";
        public string Paco2 { get { return _paco2; } set { _paco2 = value; OnPropertyChanged(); } }
        string _hco3 = "-";
        public string Hco3 { get { return _hco3; } set { _hco3 = value; OnPropertyChanged(); } }
        string _be = "-";
        public string Be { get { return _be; } set { _be = value; OnPropertyChanged(); } }
        string _po2alv = "-";
        public string Po2alv { get { return _po2alv; } set { _po2alv = value; OnPropertyChanged(); } }
        string _pco2alv = "-";
        public string Pco2alv { get { return _pco2alv; } set { _pco2alv = value; OnPropertyChanged(); } }

        private string lactateAA;

        public string LactateAA
        {
            get { return lactateAA; }
            set { lactateAA = value; OnPropertyChanged(); }
        }

        private string lactateLB;

        public string LactateLB
        {
            get { return lactateLB; }
            set { lactateLB = value; OnPropertyChanged(); }
        }
        private string lactateUB;

        public string LactateUB
        {
            get { return lactateUB; }
            set { lactateUB = value; OnPropertyChanged(); }
        }
        private string lactateBRAIN;

        public string LactateBRAIN
        {
            get { return lactateBRAIN; }
            set { lactateBRAIN = value; OnPropertyChanged(); }
        }
        private string lactateLIVER;

        public string LactateLIVER
        {
            get { return lactateLIVER; }
            set { lactateLIVER = value; OnPropertyChanged(); }
        }


        string _endtidalco2 = "-";
        public string Endtidalco2 { get { return _endtidalco2; } set { _endtidalco2 = value; OnPropertyChanged(); } }

        double _drug1Concentration = 0;
        public double Drug1Concentration { get { return _drug1Concentration; } set { _drug1Concentration = value; OnPropertyChanged(); } }

        double _drug2Concentration = 0;
        public double Drug2Concentration { get { return _drug2Concentration; } set { _drug2Concentration = value; OnPropertyChanged(); } }

        double _drug3Concentration = 0;
        public double Drug3Concentration { get { return _drug3Concentration; } set { _drug3Concentration = value; OnPropertyChanged(); } }

        double _drug4Concentration = 0;
        public double Drug4Concentration { get { return _drug4Concentration; } set { _drug4Concentration = value; OnPropertyChanged(); } }

        double _drug5Concentration = 0;
        public double Drug5Concentration { get { return _drug5Concentration; } set { _drug5Concentration = value; OnPropertyChanged(); } }

        double _drug6Concentration = 0;
        public double Drug6Concentration { get { return _drug6Concentration; } set { _drug6Concentration = value; OnPropertyChanged(); } }

        double _drug7Concentration = 0;
        public double Drug7Concentration { get { return _drug7Concentration; } set { _drug7Concentration = value; OnPropertyChanged(); } }

        double _drug8Concentration = 0;
        public double Drug8Concentration { get { return _drug8Concentration; } set { _drug8Concentration = value; OnPropertyChanged(); } }

        double _drug9Concentration = 0;
        public double Drug9Concentration { get { return _drug9Concentration; } set { _drug9Concentration = value; OnPropertyChanged(); } }

        double _drug10Concentration = 0;
        public double Drug10Concentration { get { return _drug10Concentration; } set { _drug10Concentration = value; OnPropertyChanged(); } }

        #endregion

        #region "independent model parameters setters"
        // autonomic nervous system model
        public double ThMAP
        {
            get
            {
                return currentModel != null ? currentModel.modelState.ThMAP : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.ThMAP = value;
                    OnPropertyChanged();
                }
            }
        }
        public double OpMAP
        {
            get
            {
                return currentModel != null ? currentModel.modelState.OpMAP : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.OpMAP = value;
                    OnPropertyChanged();
                }
            }
        }
        public double SaMAP
        {
            get
            {
                return currentModel != null ? currentModel.modelState.SaMAP : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.SaMAP = value;
                    OnPropertyChanged();
                }
            }
        }

        public double ThPO2
        {
            get
            {
                return currentModel != null ? currentModel.modelState.ThPO2 : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.ThPO2 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double OpPO2
        {
            get
            {
                return currentModel != null ? currentModel.modelState.OpPO2 : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.OpPO2 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double SaPO2
        {
            get
            {
                return currentModel != null ? currentModel.modelState.SaPO2 : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.SaPO2 = value;
                    OnPropertyChanged();
                }
            }
        }

        public double ThPH
        {
            get
            {
                return currentModel != null ? currentModel.modelState.ThPH : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.ThPH = value;
                    OnPropertyChanged();
                }
            }
        }
        public double OpPH
        {
            get
            {
                return currentModel != null ? currentModel.modelState.OpPH : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.OpPH = value;
                    OnPropertyChanged();
                }
            }
        }
        public double SaPH
        {
            get
            {
                return currentModel != null ? currentModel.modelState.SaPH : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.SaPH = value;
                    OnPropertyChanged();
                }
            }
        }

        public double ThPCO2
        {
            get
            {
                return currentModel != null ? currentModel.modelState.ThPCO2 : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.ThPCO2 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double OpPCO2
        {
            get
            {
                return currentModel != null ? currentModel.modelState.OpPCO2 : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.OpPCO2 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double SaPCO2
        {
            get
            {
                return currentModel != null ? currentModel.modelState.SaPCO2 : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.SaPCO2 = value;
                    OnPropertyChanged();
                }
            }
        }

        public double GPHVE
        {
            get
            {
                return currentModel != null ? currentModel.modelState.GPHVe : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.GPHVe = value;
                    OnPropertyChanged();
                }
            }
        }
        public double GPHCont
        {
            get
            {
                return currentModel != null ? currentModel.modelState.GPHCont : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.GPHCont = value;
                    OnPropertyChanged();
                }
            }
        }
        public double TcPHVE
        {
            get
            {
                return currentModel != null ? currentModel.modelState.TcPHVe : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.TcPHVe = value;
                    OnPropertyChanged();
                }
            }
        }
        public double TcPHCont
        {
            get
            {
                return currentModel != null ? currentModel.modelState.TcPHCont : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.TcPHCont = value;
                    OnPropertyChanged();
                }
            }
        }

        public double GPO2VE
        {
            get
            {
                return currentModel != null ? currentModel.modelState.GPO2Ve : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.GPO2Ve = value;
                    OnPropertyChanged();
                }
            }
        }
        public double GPO2HP
        {
            get
            {
                return currentModel != null ? currentModel.modelState.GPO2Hp : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.GPO2Hp = value;
                    OnPropertyChanged();
                }
            }
        }
        public double TcPO2VE
        {
            get
            {
                return currentModel != null ? currentModel.modelState.TcPO2Ve : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.TcPO2Ve = value;
                    OnPropertyChanged();
                }
            }
        }
        public double TcPO2HP
        {
            get
            {
                return currentModel != null ? currentModel.modelState.TcPO2Hp : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.TcPO2Hp = value;
                    OnPropertyChanged();
                }
            }
        }
        public double GPCO2VE
        {
            get
            {
                return currentModel != null ? currentModel.modelState.GPCO2Ve : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.GPCO2Ve = value;
                    OnPropertyChanged();
                }
            }
        }
        public double GPCO2HP
        {
            get
            {
                return currentModel != null ? currentModel.modelState.GPCO2Hp : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.GPCO2Hp = value;
                    OnPropertyChanged();
                }
            }
        }
        public double TcPCO2VE
        {
            get
            {
                return currentModel != null ? currentModel.modelState.TcPCO2Ve : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.TcPCO2Ve = value;
                    OnPropertyChanged();
                }
            }
        }
        public double TcPCO2HP
        {
            get
            {
                return currentModel != null ? currentModel.modelState.TcPCO2Hp : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.TcPCO2Hp = value;
                    OnPropertyChanged();
                }
            }
        }
        public double GMAPHP
        {
            get
            {
                return currentModel != null ? currentModel.modelState.GMAPHp : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.GMAPHp = value;
                    OnPropertyChanged();
                }
            }
        }
        public double TcMAPHP
        {
            get
            {
                return currentModel != null ? currentModel.modelState.TcMAPHp : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.TcMAPHp = value;
                    OnPropertyChanged();
                }
            }
        }
        public double GMAPCont
        {
            get
            {
                return currentModel != null ? currentModel.modelState.GMAPCont : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.GMAPCont = value;
                    OnPropertyChanged();
                }
            }
        }
        public double TcMAPCont
        {
            get
            {
                return currentModel != null ? currentModel.modelState.TcMAPCont : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.TcMAPCont = value;
                    OnPropertyChanged();
                }
            }
        }
        public double GMAPSVR
        {
            get
            {
                return currentModel != null ? currentModel.modelState.GMAPRes : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.GMAPRes = value;
                    OnPropertyChanged();
                }
            }
        }
        public double TcMAPSVR
        {
            get
            {
                return currentModel != null ? currentModel.modelState.TcMAPRes : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.TcMAPRes = value;
                    OnPropertyChanged();
                }
            }
        }
        public double GMAPVen
        {
            get
            {
                return currentModel != null ? currentModel.modelState.GMAPVen : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.GMAPVen = value;
                    OnPropertyChanged();
                }
            }
        }
        public double TcMAPVen
        {
            get
            {
                return currentModel != null ? currentModel.modelState.TcMAPVen : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.TcMAPVen = value;
                    OnPropertyChanged();
                }
            }
        }



        // Breathing model
        public bool SpontaneousBreathing
        {
            get
            {
                return currentModel != null ? currentModel.modelState.SpontBreathingEnabled : true;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.SpontBreathingEnabled = value;
                    OnPropertyChanged();
                }
            }
        }
        public double VERef
        {
            get
            {
                return currentModel != null ? currentModel.modelState.VERef : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.VERef = value;
                    OnPropertyChanged();
                }
            }
        }
        public double VEMax
        {
            get
            {
                return currentModel != null ? currentModel.modelState.VEMax : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.VEMax = value;
                    OnPropertyChanged();
                }
            }
        }
        public double VtRrRatio
        {
            get
            {
                return currentModel != null ? currentModel.modelState.VtRrRatio : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.VtRrRatio = value;
                    OnPropertyChanged();
                }
            }
        }
        public double BreathDuration
        {
            get
            {
                return currentModel != null ? currentModel.modelState.BreathDuration : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.BreathDuration = value;
                    OnPropertyChanged();
                }
            }
        }
        public double BreathDurationRand
        {
            get
            {
                return currentModel != null ? currentModel.modelState.BreathDurationRand : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.BreathDurationRand = value;
                    OnPropertyChanged();
                }
            }
        }
        public double BreathDepthRand
        {
            get
            {
                return currentModel != null ? currentModel.modelState.BreathDepthRand : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.BreathDepthRand = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ApnoeOccurence
        {
            get
            {
                return currentModel != null ? currentModel.modelState.ApnoeOccurence : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.ApnoeOccurence = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ApnoeDuration
        {
            get
            {
                return currentModel != null ? currentModel.modelState.ApnoeDuration : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.ApnoeDuration = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ApnoeDurationRand
        {
            get
            {
                return currentModel != null ? currentModel.modelState.ApnoeDurationRand : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.ApnoeDurationRand = value;
                    OnPropertyChanged();
                }
            }
        }

        // ecg model
        public double PQTime
        {
            get
            {
                return currentModel != null ? currentModel.modelState.PQTime : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.PQTime = value;
                    OnPropertyChanged();
                }
            }
        }
        public double QRSTime
        {
            get
            {
                return currentModel != null ? currentModel.modelState.QRSTime : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.QRSTime = value;
                    OnPropertyChanged();
                }
            }
        }
        public double QTTime
        {
            get
            {
                return currentModel != null ? currentModel.modelState.QTTime : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.QTTime = value;
                    OnPropertyChanged();
                }
            }
        }
        public double VentEscapeRate
        {
            get
            {
                return currentModel != null ? currentModel.modelState.VentEscapeRate : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.VentEscapeRate = value;
                    OnPropertyChanged();
                }
            }
        }
        public double WandPacemakerRate
        {
            get
            {
                return currentModel != null ? currentModel.modelState.WandPacemakerRate : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.WandPacemakerRate = value;
                    OnPropertyChanged();
                }
            }
        }
        public double PWaveAmp
        {
            get
            {
                return currentModel != null ? currentModel.modelState.PWaveAmp : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.PWaveAmp = value;
                    OnPropertyChanged();
                }
            }
        }
        public double TWaveAmp
        {
            get
            {
                return currentModel != null ? currentModel.modelState.TWaveAmp : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.TWaveAmp = value;
                    OnPropertyChanged();
                }
            }
        }
        public int NoiseLevel
        {
            get
            {
                return currentModel != null ? currentModel.modelState.NoiseLevel : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.NoiseLevel = value;
                    OnPropertyChanged();
                }
            }
        }
        public int RhythmType
        {
            get
            {
                return currentModel != null ? currentModel.modelState.RhythmType : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.RhythmType = value;
                    OnPropertyChanged();
                }
            }
        }

        // mechanical ventilator 
        public bool VirtualVentilatorEnabled
        {
            get
            {
                return currentModel != null ? currentModel.modelState.VirtualVentilatorEnabled : true;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.VirtualVentilatorEnabled = value;
                    currentModel.modelInterface.SwitchVirtualVentilator(value);
                    OnPropertyChanged();
                }
            }
        }
        public bool Vent_VolumeControlled
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Vent_VolumeControlled : true;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.Vent_VolumeControlled = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Vent_PIP
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Vent_PIP : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.Vent_PIP = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Vent_PEEP
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Vent_PEEP : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.Vent_PEEP = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Vent_InspFlow
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Vent_InspFlow : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.Vent_InspFlow = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Vent_ExpFlow
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Vent_ExpFlow : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.Vent_ExpFlow = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Vent_TargetTidalVolume
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Vent_TargetTidalVolume : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.Vent_TargetTidalVolume = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Vent_TIn
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Vent_TIn : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.Vent_TIn = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Vent_TOut
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Vent_TOut : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.Vent_TOut = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Vent_TriggerVolume
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Vent_TriggerVolume : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.Vent_TriggerVolume = value;
                    OnPropertyChanged();
                }
            }
        }

        public double VATP
        {
            get
            {
                return currentModel != null ? currentModel.modelState.VATPBaseline : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.VATPBaseline = value;
                    OnPropertyChanged();
                }
            }
        }

        // oxygen

        public double Hemoglobin
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Hemoglobin : 0;
            }
            set
            {
                if (currentModel.modelInterface != null)
                {
                    currentModel.modelInterface.AdjustHemoglobinConcentration(value);
                    OnPropertyChanged();
                }
            }
        }
        public double DPG
        {
            get
            {
                return currentModel != null ? currentModel.modelState.DPG_blood : 0;
            }
            set
            {
                if (currentModel.modelInterface != null)
                {
                    currentModel.modelInterface.AdjustDPGConcentration(value);
                    OnPropertyChanged();
                }
            }
        }

        // acid base
        public double RespQ
        {
            get
            {
                return currentModel != null ? currentModel.modelState.RespQ : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.RespQ = value;
                    OnPropertyChanged();
                }
            }
        }
        public double NaPlasma
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Na_Plasma : 0;
            }
            set
            {
                if (currentModel.modelInterface != null)
                {
                    currentModel.modelInterface.AdjustSodiumConcentration(value);
                    OnPropertyChanged();
                }
            }
        }
        public double KPlasma
        {
            get
            {
                return currentModel != null ? currentModel.modelState.K_Plasma : 0;
            }
            set
            {
                if (currentModel.modelInterface != null)
                {
                    currentModel.modelInterface.AdjustPotassiumConcentration(value);
                    OnPropertyChanged();
                }
            }
        }
        public double ClPlasma
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Cl_Plasma : 0;
            }
            set
            {
                if (currentModel.modelInterface != null)
                {
                    currentModel.modelInterface.AdjustChlorideConcentration(value);
                    OnPropertyChanged();
                }
            }
        }
        public double CaPlasma
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Ca_Plasma : 0;
            }
            set
            {
                if (currentModel.modelInterface != null)
                {
                    currentModel.modelInterface.AdjustCalciumConcentration(value);
                    OnPropertyChanged();
                }
            }
        }
        public double MgPlasma
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Mg_Plasma : 0;
            }
            set
            {
                if (currentModel.modelInterface != null)
                {
                    currentModel.modelInterface.AdjustMagnesiumConcentration(value);
                    OnPropertyChanged();
                }
            }
        }
        public double PhosPlasma
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Phos_Plasma : 0;
            }
            set
            {
                if (currentModel.modelInterface != null)
                {
                    currentModel.modelInterface.AdjustPhosphatesConcentration(value);
                    OnPropertyChanged();
                }
            }
        }
        public double AlbPlasma
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Alb_Plasma : 0;
            }
            set
            {
                if (currentModel.modelInterface != null)
                {
                    currentModel.modelInterface.AdjustAlbuminConcentration(value);
                    OnPropertyChanged();
                }
            }
        }
        public double UaPlasma
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Ua_Plasma : 0;
            }
            set
            {
                if (currentModel.modelInterface != null)
                {
                    currentModel.modelInterface.AdjustUnmeasuredAnionsConcentration(value);
                    OnPropertyChanged();
                }
            }
        }
        public double UcPlasma
        {
            get
            {
                return currentModel != null ? currentModel.modelState.Uc_Plasma : 0;
            }
            set
            {
                if (currentModel.modelInterface != null)
                {
                    currentModel.modelInterface.AdjustUnmeasuredCationsConcentration(value);
                    OnPropertyChanged();
                }
            }
        }

        public double LactateClearanceRate
        {
            get
            {
                return currentModel != null ? currentModel.modelState.LactateClearanceRate / currentModel.modelState.ModelingStepsize : 0;
            }
            set
            {
                if (currentModel.modelInterface != null)
                {
                    currentModel.modelState.LactateClearanceRate = value * currentModel.modelState.ModelingStepsize;
                    OnPropertyChanged();
                }
            }
        }



        // lung and chestwall model

        public double Resp_UAR_Insp
        {
            get
            {
                return currentModel != null ? currentModel.modelState.OUT_NCA.resistance.RForwardBaseline : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.OUT_NCA.resistance.RForwardBaseline = value;

                    OnPropertyChanged();
                }
            }
        }
        public double Resp_UAR_Exp
        {
            get
            {
                return currentModel != null ? currentModel.modelState.OUT_NCA.resistance.RBackwardBaseline : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.OUT_NCA.resistance.RBackwardBaseline = value;

                    OnPropertyChanged();
                }
            }
        }
        public double Resp_LARR_Insp
        {
            get
            {
                return currentModel != null ? currentModel.modelState.NCA_ALR.resistance.RForwardBaseline : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.NCA_ALR.resistance.RForwardBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Resp_LARR_Exp
        {
            get
            {
                return currentModel != null ? currentModel.modelState.NCA_ALR.resistance.RBackwardBaseline : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.NCA_ALR.resistance.RBackwardBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Resp_LARL_Insp
        {
            get
            {
                return currentModel != null ? currentModel.modelState.NCA_ALL.resistance.RForwardBaseline : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.NCA_ALL.resistance.RForwardBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Resp_LARL_Exp
        {
            get
            {
                return currentModel != null ? currentModel.modelState.NCA_ALL.resistance.RBackwardBaseline : 0;
            }
            set
            {
                if (currentModel != null)
                {
                    currentModel.modelState.NCA_ALL.resistance.RBackwardBaseline = value;
                    OnPropertyChanged();
                }
            }
        }

        double _lungComplianceChange = 0;
        public double LungComplianceChange
        {
            get
            {
                return _lungComplianceChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {
                    _lungComplianceChange = value;
                    currentModel.modelInterface.AdjustLungCompliance(value);

                    OnPropertyChanged();
                }
                else
                {
                    _lungComplianceChange = 0;
                }
            }
        }
        

        double _upperAirwayResistanceChange = 0;
        public double UpperAirwayResistanceChange
        {
            get
            {
                return _upperAirwayResistanceChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {
                    _upperAirwayResistanceChange = value;
                    currentModel.modelInterface.AdjustUpperAirwayResistance(value);

                    OnPropertyChanged();
                }
                else
                {
                    _upperAirwayResistanceChange = 0;
                }
            }
        }
        double _etTubeResistanceChange = 0;
        public double EtTubeResistanceChange
        {
            get
            {
                return _etTubeResistanceChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {
                    _etTubeResistanceChange = value;
                    currentModel.modelInterface.AdjustETTubeResistance(value);

                    OnPropertyChanged();
                }
                else
                {
                    _etTubeResistanceChange = 0;
                }
            }
        }

        double _lowerAirwayResistanceChange = 0;
        public double LowerAirwayResistanceChange
        {
            get
            {
                return _lowerAirwayResistanceChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {
                    _lowerAirwayResistanceChange = value;
                    currentModel.modelInterface.AdjustLowerAirwayResistance(value);

                    OnPropertyChanged();
                }
                else
                {
                    _lowerAirwayResistanceChange = 0;
                }
            }
        }


        double _airwayComplianceChange = 0;
        public double AirwayComplianceChange
        {
            get
            {
                return _airwayComplianceChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {
                    _airwayComplianceChange = value;
                    currentModel.modelInterface.AdjustAirwayCompliance(value);

                    OnPropertyChanged();
                }
                else
                {
                    _airwayComplianceChange = 0;
                }
            }
        }
        double _chestwallComplianceChange = 0;
        public double ChestwallComplianceChange
        {
            get
            {
                return _chestwallComplianceChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {
                    _chestwallComplianceChange = value;
                    currentModel.modelInterface.AdjustChestwallCompliance(value);

                    OnPropertyChanged();
                }
                else
                {
                    _chestwallComplianceChange = 0;
                }
            }
        }

        double _lungDiffCapacityChange = 0;
        public double LungDiffusionCapacityChange
        {
            get
            {
                return _lungDiffCapacityChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {
                    _lungDiffCapacityChange = value;
                    currentModel.modelInterface.AdjustLungDiffusionCapacity(value);

                    OnPropertyChanged();
                }
                else
                {
                    _lungDiffCapacityChange = 0;
                }
            }
        }

        double _svrChange = 0;
        public double SystemicVascularResistanceChange
        {
            get
            {
                return _svrChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _svrChange = value;

                    currentModel.modelInterface.AdjustSystemicVascularResistance(value);

                    OnPropertyChanged();
                }
                else
                {
                    _svrChange = 0;
                }
            }
        }

        double _pvrChange = 0;
        public double PulmonaryVascularResistanceChange
        {
            get
            {
                return _pvrChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _pvrChange = value;

                    currentModel.modelInterface.AdjustPulmonaryVascularResistance(value);

                    OnPropertyChanged();
                }
                else
                {
                    _pvrChange = 0;
                }
            }
        }

        double _venPoolChange = 0;
        public double VenousPoolChange
        {
            get
            {
                return _venPoolChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _venPoolChange = value;

                    currentModel.modelInterface.AdjustVenousPool(value);

                    OnPropertyChanged();
                }
                else
                {
                    _venPoolChange = 0;
                }
            }
        }

        double _heartDiastFunction = 0;
        public double HeartDiastolicFunctionChange
        {
            get
            {
                return _heartDiastFunction;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _heartDiastFunction = value;

                    currentModel.modelInterface.AdjustHeartDiastolicFunction(value);

                    OnPropertyChanged();
                }
                else
                {
                    _heartDiastFunction = 0;
                }
            }
        }

        double _heartLeftDiastFunction = 0;
        public double HeartLeftDiastolicFunctionChange
        {
            get
            {
                return _heartLeftDiastFunction;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _heartLeftDiastFunction = value;

                    currentModel.modelInterface.AdjustHeartLeftDiastolicFunction(value);

                    OnPropertyChanged();
                }
                else
                {
                    _heartLeftDiastFunction = 0;
                }
            }
        }
        double _heartRightDiastFunction = 0;
        public double HeartRightDiastolicFunctionChange
        {
            get
            {
                return _heartRightDiastFunction;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _heartRightDiastFunction = value;

                    currentModel.modelInterface.AdjustHeartRightDiastolicFunction(value);

                    OnPropertyChanged();
                }
                else
                {
                    _heartRightDiastFunction = 0;
                }
            }
        }

        double _heartCont = 0;
        public double HeartContractilityChange
        {
            get
            {
                return _heartCont;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _heartCont = value;

                    currentModel.modelInterface.AdjustHeartContractility(value);

                    OnPropertyChanged();
                }
                else
                {
                    _heartCont = 0;
                }
            }
        }

        double _heartLeftCont = 0;
        public double HeartLeftContractilityChange
        {
            get
            {
                return _heartLeftCont;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _heartLeftCont = value;

                    currentModel.modelInterface.AdjustLeftHeartContractility(value);

                    OnPropertyChanged();
                }
                else
                {
                    _heartLeftCont = 0;
                }
            }
        }
        double _heartRightCont = 0;
        public double HeartRightContractilityChange
        {
            get
            {
                return _heartRightCont;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _heartRightCont = value;

                    currentModel.modelInterface.AdjustRightHeartContractility(value);

                    OnPropertyChanged();
                }
                else
                {
                    _heartRightCont = 0;
                }
            }
        }

        double _avValveStenosisChange = 0;
        public double AVStenosisChange
        {
            get
            {
                return _avValveStenosisChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _avValveStenosisChange = value;

                    currentModel.modelInterface.AdjustAVValveStenosis(value);

                    OnPropertyChanged();
                }
                else
                {
                    _avValveStenosisChange = 0;
                }
            }
        }

        double _avValveRegurgitationChange = 0;
        public double AVRegurgitationChange
        {
            get
            {
                return _avValveRegurgitationChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _avValveRegurgitationChange = value;

                    currentModel.modelInterface.AdjustAVValveRegurgitation(value);

                    OnPropertyChanged();
                }
                else
                {
                    _avValveRegurgitationChange = 0;
                }
            }
        }

        double _pvValveStenosisChange = 0;
        public double PVStenosisChange
        {
            get
            {
                return _pvValveStenosisChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _pvValveStenosisChange = value;

                    currentModel.modelInterface.AdjustPVValveStenosis(value);

                    OnPropertyChanged();
                }
                else
                {
                    _pvValveStenosisChange = 0;
                }
            }
        }

        double _pvValveRegurgitationChange = 0;
        public double PVRegurgitationChange
        {
            get
            {
                return _pvValveRegurgitationChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _pvValveRegurgitationChange = value;

                    currentModel.modelInterface.AdjustPVValveRegurgitation(value);

                    OnPropertyChanged();
                }
                else
                {
                    _pvValveRegurgitationChange = 0;
                }
            }
        }

        double _mvValveStenosisChange = 0;
        public double MVStenosisChange
        {
            get
            {
                return _mvValveStenosisChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _mvValveStenosisChange = value;

                    currentModel.modelInterface.AdjustMVValveStenosis(value);

                    OnPropertyChanged();
                }
                else
                {
                    _mvValveStenosisChange = 0;
                }
            }
        }

        double _mvValveRegurgitationChange = 0;
        public double MVRegurgitationChange
        {
            get
            {
                return _mvValveRegurgitationChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _mvValveRegurgitationChange = value;

                    currentModel.modelInterface.AdjustMVValveRegurgitation(value);

                    OnPropertyChanged();
                }
                else
                {
                    _mvValveRegurgitationChange = 0;
                }
            }
        }

        double _tvValveStenosisChange = 0;
        public double TVStenosisChange
        {
            get
            {
                return _tvValveStenosisChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _tvValveStenosisChange = value;

                    currentModel.modelInterface.AdjustTVValveStenosis(value);

                    OnPropertyChanged();
                }
                else
                {
                    _tvValveStenosisChange = 0;
                }
            }
        }

        double _tvValveRegurgitationChange = 0;
        public double TVRegurgitationChange
        {
            get
            {
                return _tvValveRegurgitationChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _tvValveRegurgitationChange = value;

                    currentModel.modelInterface.AdjustTVValveRegurgitation(value);

                    OnPropertyChanged();
                }
                else
                {
                    _tvValveRegurgitationChange = 0;
                }
            }
        }

        double _pericardiumComplianceChange = 0;
        public double PericardiumComplianceChange
        {
            get
            {
                return _pericardiumComplianceChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {
                    _pericardiumComplianceChange = value;
                    currentModel.modelInterface.AdjustPericardialCompliance(value);

                    OnPropertyChanged();
                }
                else
                {
                    _pericardiumComplianceChange = 0;
                }
            }
        }

        double _bloodVolumeChange = 0;
        public double BloodVolumeChange
        {
            get
            {
                return _bloodVolumeChange;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _bloodVolumeChange = value;

                    currentModel.modelInterface.AdjustBloodVolume(value);

                    OnPropertyChanged();
                }
                else
                {
                    _bloodVolumeChange = 0;
                }
            }
        }

        double _pdaSize = 0;
        public double PDASize
        {
            get
            {
                return _pdaSize;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _pdaSize = value;

                    currentModel.modelInterface.AdjustPDASize(value);

                    OnPropertyChanged();
                }
                else
                {
                    _pdaSize = 0;
                }
            }
        }

        double _ofoSize = 0;
        public double OFOSize
        {
            get
            {
                return _ofoSize;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _ofoSize = value;

                    currentModel.modelInterface.AdjustOFOSize(value);

                    OnPropertyChanged();
                }
                else
                {
                    _ofoSize = 0;
                }
            }
        }
        double _vsdSize = 0;
        public double VSDSize
        {
            get
            {
                return _vsdSize;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _vsdSize = value;

                    currentModel.modelInterface.AdjustVSDSize(value);

                    OnPropertyChanged();
                }
                else
                {
                    _vsdSize = 0;
                }
            }
        }

        double _lungShuntSize = 0;
        public double LUNGShuntSize
        {
            get
            {
                return _lungShuntSize;
            }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _lungShuntSize = value;

                    currentModel.modelInterface.AdjustLungShuntSize(value);

                    OnPropertyChanged();
                }
                else
                {
                    _lungShuntSize = 0;
                }
            }
        }

        private double _fiO2 = 0.21;

        public double FiO2
        {
            get { return _fiO2; }
            set
            {
                if (currentModel.modelInterface != null && !double.IsNaN(value))
                {

                    _fiO2 = value;

                    currentModel.modelInterface.AdjustFiO2(value);

                    OnPropertyChanged();
                }
                else
                {
                    _fiO2 = 0.21;
                }
            }
        }

        public ObservableCollection<DrugEffect> drugEffects { get; set; } = new ObservableCollection<DrugEffect>();
        public ObservableCollection<Drug> availableDrugs { get; set; } = new ObservableCollection<Drug>();
        public ObservableCollection<Compartment> bloodcompartments { get; set; } = new ObservableCollection<Compartment>();
        public ObservableCollection<Compartment> gascompartments { get; set; } = new ObservableCollection<Compartment>();
        public ObservableCollection<Connector> connectors { get; set; } = new ObservableCollection<Connector>();
        public ObservableCollection<ContainerCompartment> containers { get; set; } = new ObservableCollection<ContainerCompartment>();
        public ObservableCollection<Compartment> containedCompartments { get; set; } = new ObservableCollection<Compartment>();

        public ObservableCollection<GasExchangeBlock> gasexchangeUnits { get; set; } = new ObservableCollection<GasExchangeBlock>();
        public ObservableCollection<string> rhythmTypes { get; set; } = new ObservableCollection<string>();

        #endregion

        #region "other couplings"
            ObservableCollection<string> _modelLog = new ObservableCollection<string>();

            public ObservableCollection<string> ModelLog { get { return _modelLog; } set { _modelLog = value; OnPropertyChanged(); } }
        #endregion

        #region "graphs"

        void UpdatePatientMonitor()
        {
            if (MonitorVisible && GraphPatientMonitor != null)
            {
                GraphPatientMonitor.UpdateCurves(currentModel.modelInterface.ECGSignal, currentModel.modelInterface.ABPSignal,
                                                 currentModel.modelInterface.SPO2POSTSignal, currentModel.modelInterface.ETCO2Signal,
                                                 currentModel.modelInterface.RESPVolumeSignal);

            }
        }
        void UpdatePVLoopGraph()
        {
            if (GraphPVLoop != null && PVLoopVisible)
            {
                GraphPVLoop.WriteListToBuffer(currentModel.analyzer.pressuresBloodCompartment, currentModel.analyzer.volumesBloodCompartment);
            }
        
        }
        public void InitPVLoop(LoopGraph p)
        {
            GraphPVLoop = p;
            GraphPVLoop.GraphTitle = "pv loop";
            GraphPVLoop.GraphTitleColor = new SolidColorBrush(Colors.LimeGreen);
            GraphPVLoop.GraphPaint1.Color = SKColors.LimeGreen;
            GraphPVLoop.GridYMax = 100;
            GraphPVLoop.GridYStep = 10;
            GraphPVLoop.GridXStep = 5;
            GraphPVLoop.GridYMin = 0;
            GraphPVLoop.GridXMax = 25;
            GraphPVLoop.GridXMin = 0;
            GraphPVLoop.GridXEnabled = true;
            GraphPVLoop.GridYEnabled = true;

        }
        public void InitPatientMonitor(PatientMonitor p)
        {
            GraphPatientMonitor = p;
            GraphPatientMonitor.InitPatientMonitor();
        }

        public void InitModelDiagram(ModelDiagram p)
        {
            GraphModelDiagram = p;
            BuildModelDiagram();    

        }
        void BuildModelDiagram()
        {
            GraphModelDiagram.InitModelDiagram(currentModel);
            GraphModelDiagram.BuildDiagram();
            GraphModelDiagram.UpdateSkeleton();
            GraphModelDiagram.UpdatedMainDiagram();
        }
        public void InitTrendGraph(TimeBasedGraph p)
        {
            TrendGraph = p;

            TrendGraph.InitGraph(300, 400);
            TrendGraph.GridXStep = 60;
            TrendGraph.GridYStep = 25;
            TrendGraph.MaxY = 200;
            TrendGraph.ShowXLabels = true;
            TrendGraph.ShowYLabels = true;

            TrendGraph.Data1Enabled = true;
            TrendGraph.Data2Enabled = true;
            TrendGraph.Data3Enabled = true;
            TrendGraph.Data4Enabled = true;
            TrendGraph.Data5Enabled = true;
          
        }
        public void InitBloodgasGraph(TimeBasedGraph p)
        {
            BloodgasGraph = p;

            BloodgasGraph.InitGraph(300, 400);
            BloodgasGraph.GridXStep = 60;
            BloodgasGraph.GridYStep = 1;
            BloodgasGraph.MaxY = 15;
            BloodgasGraph.ShowXLabels = true;
            BloodgasGraph.ShowYLabels = true;

            BloodgasGraph.Data1Enabled = true;
            BloodgasGraph.Data2Enabled = true;
            BloodgasGraph.Data3Enabled = true;
            BloodgasGraph.Data4Enabled = true;
            BloodgasGraph.Data5Enabled = true;
        }
        void UpdateTrendGraph()
        {
            if (TrendGraph != null)
            {

                TrendGraph.UpdateData(currentModel.modelInterface.HeartRate, currentModel.modelInterface.PulseOximeterOutput, currentModel.modelInterface.SystolicSystemicArterialPressure, currentModel.modelInterface.DiastolicSystemicArterialPressure, currentModel.modelInterface.RespiratoryRate);

            }
        }
        void UpdateBloodgasGraph()
        {
            if (BloodgasGraph != null)
            {

                BloodgasGraph.UpdateData(currentModel.modelInterface.ArterialPH, currentModel.modelInterface.ArterialPCO2, currentModel.modelInterface.ArterialPO2, currentModel.modelInterface.ArterialBE, currentModel.modelInterface.ArterialLactate);

            }
        }

        #endregion

        #region "bloodcompartment settings"
        void ChangeSelectedBloodCompartment(object p)
        {
            
            
            selectedBloodCompartment = (BloodCompartment)p;
            currentModel.analyzer.SelectBloodCompartment(selectedBloodCompartment);

            GraphPVLoop.GridYMax = (float)selectedBloodCompartment.dataCollector.PresMax + 0.25f * (float)selectedBloodCompartment.dataCollector.PresMax;
            GraphPVLoop.GridYMin = (float)selectedBloodCompartment.dataCollector.PresMin - 0.25f * (float)selectedBloodCompartment.dataCollector.PresMin;
            GraphPVLoop.GridXMax = (float)selectedBloodCompartment.dataCollector.VolMax + 0.25f * (float)selectedBloodCompartment.dataCollector.VolMax;
            GraphPVLoop.GridXMin = (float)selectedBloodCompartment.dataCollector.VolMin - 0.25f * (float)selectedBloodCompartment.dataCollector.VolMin;
            GraphPVLoop.refresh = true;


            if (selectedBloodCompartment != null)
            {
                UVolBlood = selectedBloodCompartment.VolUBaseline;
                ElBaselineBlood = selectedBloodCompartment.elastanceModel.ElBaseline;
                ElContractionBaselineBlood = selectedBloodCompartment.elastanceModel.ElContractionBaseline;
                ElKMinVolumeBlood = selectedBloodCompartment.elastanceModel.ElKMinVolume;
                ElK1Blood = selectedBloodCompartment.elastanceModel.ElK1;
                ElKMaxVolumeBlood = selectedBloodCompartment.elastanceModel.ElKMaxVolume;
                ElK2Blood = selectedBloodCompartment.elastanceModel.ElK2;
                FVATP = selectedBloodCompartment.LocalVATPFactor;
                IsEnabledBlood = selectedBloodCompartment.IsEnabled;
                HasFixedVolumeBlood = selectedBloodCompartment.HasFixedVolume;
            }
           

        }
        public double UVolBlood
        {
            get
            {
                return selectedBloodCompartment != null ? selectedBloodCompartment.VolUBaseline : 0;
            }
            set
            {
                if (selectedBloodCompartment != null)
                {
                    selectedBloodCompartment.VolUBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElBaselineBlood
        {
            get
            {
                return selectedBloodCompartment != null ? selectedBloodCompartment.elastanceModel.ElBaseline : 0;
            }
            set
            {
                if (selectedBloodCompartment != null)
                {
                    selectedBloodCompartment.elastanceModel.ElBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElContractionBaselineBlood
        {
            get
            {
                return selectedBloodCompartment != null ? selectedBloodCompartment.elastanceModel.ElContractionBaseline : 0;
            }
            set
            {
                if (selectedBloodCompartment != null)
                {
                    selectedBloodCompartment.elastanceModel.ElContractionBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElKMinVolumeBlood
        {
            get
            {
                return selectedBloodCompartment != null ? selectedBloodCompartment.elastanceModel.ElKMinVolume : 0;
            }
            set
            {
                if (selectedBloodCompartment != null)
                {
                    selectedBloodCompartment.elastanceModel.ElKMinVolume = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElK1Blood
        {
            get
            {
                return selectedBloodCompartment != null ? selectedBloodCompartment.elastanceModel.ElK1 : 0;
            }
            set
            {
                if (selectedBloodCompartment != null)
                {
                    selectedBloodCompartment.elastanceModel.ElK1 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElKMaxVolumeBlood
        {
            get
            {
                return selectedBloodCompartment != null ? selectedBloodCompartment.elastanceModel.ElKMaxVolume : 0;
            }
            set
            {
                if (selectedBloodCompartment != null)
                {
                    selectedBloodCompartment.elastanceModel.ElKMaxVolume = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElK2Blood
        {
            get
            {
                return selectedBloodCompartment != null ? selectedBloodCompartment.elastanceModel.ElK2 : 0;
            }
            set
            {
                if (selectedBloodCompartment != null)
                {
                    selectedBloodCompartment.elastanceModel.ElK2 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double FVATP
        {
            get
            {
                return selectedBloodCompartment != null ? selectedBloodCompartment.LocalVATPFactor : 0;
            }
            set
            {
                if (selectedBloodCompartment != null)
                {
                    selectedBloodCompartment.LocalVATPFactor = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsEnabledBlood
        {
            get
            {
                return selectedBloodCompartment != null ? selectedBloodCompartment.IsEnabled : true;
            }
            set
            {
                if (selectedBloodCompartment != null)
                {
                    selectedBloodCompartment.IsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool HasFixedVolumeBlood
        {
            get
            {
                return selectedBloodCompartment != null ? selectedBloodCompartment.HasFixedVolume : true;
            }
            set
            {
                if (selectedBloodCompartment != null)
                {
                    selectedBloodCompartment.HasFixedVolume = value;
                    OnPropertyChanged();
                }
            }
        }

        public double DrugHepaticClearanceRate
        {
            get
            {
                return selectedDrug != null ? selectedDrug.HepaticClearanceRate : 0;
            }
            set
            {
                if (selectedDrug != null)
                {
                    selectedDrug.HepaticClearanceRate = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DrugRenalClearanceRate
        {
            get
            {
                return selectedDrug != null ? selectedDrug.RenalClearanceRate : 0;
            }
            set
            {
                if (selectedDrug != null)
                {
                    selectedDrug.RenalClearanceRate = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DrugMetabolicRate
        {
            get
            {
                return selectedDrug != null ? selectedDrug.MetabolicRate : 0;
            }
            set
            {
                if (selectedDrug != null)
                {
                    selectedDrug.MetabolicRate = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DrugDose
        {
            get
            {
                return selectedDrug != null ? selectedDrug.Dose : 0;
            }
            set
            {
                if (selectedDrug != null)
                {
                    selectedDrug.Dose = value;
                    OnPropertyChanged();
                }
            }
        }
        public string DrugEffectSite
        {
            get
            {
                return selectedDrugEffect != null ? selectedDrugEffect.EffectSite : "";
            }
            set
            {
                if (selectedDrugEffect != null)
                {
                    selectedDrugEffect.EffectSite = value;
                    OnPropertyChanged();
                }
            }
        }

        public double DrugEffectSaturation
        {
            get
            {
                return selectedDrugEffect != null ? selectedDrugEffect.EffectConcentrationSaturation : 0;
            }
            set
            {
                if (selectedDrugEffect != null)
                {
                    selectedDrugEffect.EffectConcentrationSaturation = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DrugEffectThreshold
        {
            get
            {
                return selectedDrugEffect != null ? selectedDrugEffect.EffectConcentrationThreshold : 0;
            }
            set
            {
                if (selectedDrugEffect != null)
                {
                    selectedDrugEffect.EffectConcentrationThreshold = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DrugEffectTimeConstant
        {
            get
            {
                return selectedDrugEffect != null ? selectedDrugEffect.TimeConstant : 0;
            }
            set
            {
                if (selectedDrugEffect != null)
                {
                    selectedDrugEffect.TimeConstant = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DrugEffectGain
        {
            get
            {
                return selectedDrugEffect != null ? selectedDrugEffect.Gain : 0;
            }
            set
            {
                if (selectedDrugEffect != null)
                {
                    selectedDrugEffect.Gain = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool DrugEffectDoseDependent
        {
            get
            {
                return selectedDrugEffect != null ? selectedDrugEffect.DoseDependent : true;
            }
            set
            {
                if (selectedDrugEffect != null)
                {
                    selectedDrugEffect.DoseDependent = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region "gascompartment settings"
        void ChangeSelectedGasCompartment(object p)
        {
            selectedGasCompartment = (GasCompartment)p;
            if (selectedGasCompartment != null)
            {
                UVolGas = selectedGasCompartment.VolUBaseline;
                ElBaselineGas = selectedGasCompartment.elastanceModel.ElBaseline;
                ElContractionBaselineGas = selectedGasCompartment.elastanceModel.ElContractionBaseline;
                ElKMinVolumeGas = selectedGasCompartment.elastanceModel.ElKMinVolume;
                ElK1Gas = selectedGasCompartment.elastanceModel.ElK1;
                ElKMaxVolumeGas = selectedGasCompartment.elastanceModel.ElKMaxVolume;
                ElK2Gas = selectedGasCompartment.elastanceModel.ElK2;
                IsEnabledGas = selectedGasCompartment.IsEnabled;
                HasFixedVolumeGas = selectedGasCompartment.HasFixedVolume;
                HasFixedCompositionGas = selectedGasCompartment.FixedGasComposition;
            }
   
        }
        public double UVolGas
        {
            get
            {
                return selectedGasCompartment != null ? selectedGasCompartment.VolUBaseline : 0;
            }
            set
            {
                if (selectedGasCompartment != null)
                {
                    selectedGasCompartment.VolUBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElBaselineGas
        {
            get
            {
                return selectedGasCompartment != null ? selectedGasCompartment.elastanceModel.ElBaseline : 0;
            }
            set
            {
                if (selectedGasCompartment != null)
                {
                    selectedGasCompartment.elastanceModel.ElBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElContractionBaselineGas
        {
            get
            {
                return selectedGasCompartment != null ? selectedGasCompartment.elastanceModel.ElContractionBaseline : 0;
            }
            set
            {
                if (selectedGasCompartment != null)
                {
                    selectedGasCompartment.elastanceModel.ElContractionBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElKMinVolumeGas
        {
            get
            {
                return selectedGasCompartment != null ? selectedGasCompartment.elastanceModel.ElKMinVolume : 0;
            }
            set
            {
                if (selectedGasCompartment != null)
                {
                    selectedGasCompartment.elastanceModel.ElKMinVolume = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElK1Gas
        {
            get
            {
                return selectedGasCompartment != null ? selectedGasCompartment.elastanceModel.ElK1 : 0;
            }
            set
            {
                if (selectedGasCompartment != null)
                {
                    selectedGasCompartment.elastanceModel.ElK1 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElKMaxVolumeGas
        {
            get
            {
                return selectedGasCompartment != null ? selectedGasCompartment.elastanceModel.ElKMaxVolume : 0;
            }
            set
            {
                if (selectedGasCompartment != null)
                {
                    selectedGasCompartment.elastanceModel.ElKMaxVolume = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElK2Gas
        {
            get
            {
                return selectedGasCompartment != null ? selectedGasCompartment.elastanceModel.ElK2 : 0;
            }
            set
            {
                if (selectedGasCompartment != null)
                {
                    selectedGasCompartment.elastanceModel.ElK2 = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsEnabledGas
        {
            get
            {
                return selectedGasCompartment != null ? selectedGasCompartment.IsEnabled : true;
            }
            set
            {
                if (selectedGasCompartment != null)
                {
                    selectedGasCompartment.IsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool HasFixedVolumeGas
        {
            get
            {
                return selectedGasCompartment != null ? selectedGasCompartment.HasFixedVolume : true;
            }
            set
            {
                if (selectedGasCompartment != null)
                {
                    selectedGasCompartment.HasFixedVolume = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool HasFixedCompositionGas
        {
            get
            {
                return selectedGasCompartment != null ? selectedGasCompartment.FixedGasComposition : true;
            }
            set
            {
                if (selectedGasCompartment != null)
                {
                    selectedGasCompartment.FixedGasComposition = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region "bloodcompartment connector"
        void ChangeSelectedConnector(object p)
        {
            selectedConnector = (Connector)p;
            if (selectedConnector != null)
            {
                ResForward = selectedConnector.resistance.RForwardBaseline;
                ResBackward = selectedConnector.resistance.RBackwardBaseline;
                ResK1 = selectedConnector.resistance.RK1;
                ResK2 = selectedConnector.resistance.RK2;
                IsCoupledRes = selectedConnector.resistance.ResCoupled;
                NoBackFlowRes = selectedConnector.NoBackFlow;
                IsEnabledRes = selectedConnector.IsEnabled;
            }
        

        }
        public double ResForward
        {
            get
            {
                return selectedConnector != null ? selectedConnector.resistance.RForwardBaseline : 0;
            }
            set
            {
                if (selectedConnector != null)
                {
                    selectedConnector.resistance.RForwardBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ResBackward
        {
            get
            {
                return selectedConnector != null ? selectedConnector.resistance.RBackwardBaseline : 0;
            }
            set
            {
                if (selectedConnector != null)
                {
                    selectedConnector.resistance.RBackwardBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ResK1
        {
            get
            {
                return selectedConnector != null ? selectedConnector.resistance.RK1 : 0;
            }
            set
            {
                if (selectedConnector != null)
                {
                    selectedConnector.resistance.RK1 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ResK2
        {
            get
            {
                return selectedConnector != null ? selectedConnector.resistance.RK2 : 0;
            }
            set
            {
                if (selectedConnector != null)
                {
                    selectedConnector.resistance.RK2 = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsCoupledRes
        {
            get
            {
                return selectedConnector != null ? selectedConnector.resistance.ResCoupled : false;
            }
            set
            {
                if (selectedConnector != null)
                {
                    selectedConnector.resistance.ResCoupled = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool NoBackFlowRes
        {
            get
            {
                return selectedConnector != null ? selectedConnector.NoBackFlow : false;
            }
            set
            {
                if (selectedConnector != null)
                {
                    selectedConnector.NoBackFlow = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsEnabledRes
        {
            get
            {
                return selectedConnector != null ? selectedConnector.IsEnabled : false;
            }
            set
            {
                if (selectedConnector != null)
                {
                    selectedConnector.IsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region "container compartments"
        void ChangeSelectedContainer(object p)
        {
            selectedContainer = (ContainerCompartment)p;
            if (selectedContainer != null)
            {
                UVolCont = selectedContainer.VolUBaseline;
                ElBaselineCont = selectedContainer.elastanceModel.ElBaseline;
                ElContractionBaselineCont = selectedContainer.elastanceModel.ElContractionBaseline;
                ElKMinVolumeCont = selectedContainer.elastanceModel.ElKMinVolume;
                ElK1Cont = selectedContainer.elastanceModel.ElK1;
                ElKMaxVolumeCont = selectedContainer.elastanceModel.ElKMaxVolume;
                ElK2Cont = selectedContainer.elastanceModel.ElK2;

                foreach (Compartment c in selectedContainer.bloodCompartments)
                {
                    containedCompartments.Add(c);
                }
                foreach (Compartment c in selectedContainer.gasCompartments)
                {
                    containedCompartments.Add(c);
                }
            }
           

        }
        public double UVolCont
        {
            get
            {
                return selectedContainer != null ? selectedContainer.VolUBaseline : 0;
            }
            set
            {
                if (selectedContainer != null)
                {
                    selectedContainer.VolUBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElBaselineCont
        {
            get
            {
                return selectedContainer != null ? selectedContainer.elastanceModel.ElBaseline : 0;
            }
            set
            {
                if (selectedContainer != null)
                {
                    selectedContainer.elastanceModel.ElBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElContractionBaselineCont
        {
            get
            {
                return selectedContainer != null ? selectedContainer.elastanceModel.ElContractionBaseline : 0;
            }
            set
            {
                if (selectedContainer != null)
                {
                    selectedContainer.elastanceModel.ElContractionBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElKMinVolumeCont
        {
            get
            {
                return selectedContainer != null ? selectedContainer.elastanceModel.ElKMinVolume : 0;
            }
            set
            {
                if (selectedContainer != null)
                {
                    selectedContainer.elastanceModel.ElKMinVolume = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElK1Cont
        {
            get
            {
                return selectedContainer != null ? selectedContainer.elastanceModel.ElK1 : 0;
            }
            set
            {
                if (selectedContainer != null)
                {
                    selectedContainer.elastanceModel.ElK1 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElKMaxVolumeCont
        {
            get
            {
                return selectedContainer != null ? selectedContainer.elastanceModel.ElKMaxVolume : 0;
            }
            set
            {
                if (selectedContainer != null)
                {
                    selectedContainer.elastanceModel.ElKMaxVolume = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ElK2Cont
        {
            get
            {
                return selectedContainer != null ? selectedContainer.elastanceModel.ElK2 : 0;
            }
            set
            {
                if (selectedContainer != null)
                {
                    selectedContainer.elastanceModel.ElK2 = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region "gasexchange units"
        void ChangeSelectedGex(object p)
        {
            selectedGex = (GasExchangeBlock)p;
            if (selectedGex != null)
            {
                CompBloodGex = selectedGex.CompBlood.Description;
                CompGasGex = selectedGex.CompGas.Description;
                DiffO2Gex = selectedGex.DiffCoO2Baseline;
                DiffCO2Gex = selectedGex.DiffCoCo2Baseline;
                DiffN2Gex = selectedGex.DiffCoN2Baseline;
                DiffOtherGex = selectedGex.DiffCoOtherBaseline;
            }
        }
        public string CompBloodGex
        {
            get
            {
                return selectedGex != null ? selectedGex.CompBlood.Description : "";
            }
            set
            {
                if (selectedGex != null)
                {
                    selectedGex.CompBlood.Description = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CompGasGex
        {
            get
            {
                return selectedGex != null ? selectedGex.CompGas.Description : "";
            }
            set
            {
                if (selectedGex != null)
                {
                    selectedGex.CompGas.Description = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DiffO2Gex
        {
            get
            {
                return selectedGex != null ? selectedGex.DiffCoO2Baseline : 0;
            }
            set
            {
                if (selectedGex != null)
                {
                    selectedGex.DiffCoO2Baseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DiffCO2Gex
        {
            get
            {
                return selectedGex != null ? selectedGex.DiffCoCo2Baseline : 0;
            }
            set
            {
                if (selectedGex != null)
                {
                    selectedGex.DiffCoCo2Baseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DiffN2Gex
        {
            get
            {
                return selectedGex != null ? selectedGex.DiffCoN2Baseline : 0;
            }
            set
            {
                if (selectedGex != null)
                {
                    selectedGex.DiffCoN2Baseline = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DiffOtherGex
        {
            get
            {
                return selectedGex != null ? selectedGex.DiffCoOtherBaseline : 0;
            }
            set
            {
                if (selectedGex != null)
                {
                    selectedGex.DiffCoOtherBaseline = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

    }
}
