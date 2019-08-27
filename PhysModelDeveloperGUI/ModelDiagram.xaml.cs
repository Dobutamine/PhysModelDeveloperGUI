﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PhysModelLibrary;
using SkiaSharp;

namespace PhysModelDeveloperGUI
{
    /// <summary>
    /// Interaction logic for ModelDiagram.xaml
    /// </summary>
    public partial class ModelDiagram : UserControl
    {
        Model currentModel;

        public List<AnimatedBloodCompartment> animatedBloodCompartments = new List<AnimatedBloodCompartment>();
        public List<AnimatedGasComp> animatedGasCompartments = new List<AnimatedGasComp>();
        public List<AnimatedBloodConnector> animatedBloodConnectors = new List<AnimatedBloodConnector>();
        public List<AnimatedGasConnector> animatedGasConnectors = new List<AnimatedGasConnector>();
        public List<AnimatedValve> animatedValves = new List<AnimatedValve>();
        public List<AnimatedShunt> animatedShunts = new List<AnimatedShunt>();
        public List<AnimatedShuntGas> animatedShuntsGas = new List<AnimatedShuntGas>();

        public AnimatedBloodCompartment myocardium;
        public AnimatedBloodConnector AAMYO;
        public AnimatedBloodConnector MYORA;
        public AnimatedBloodConnector LUNGPV;
        public AnimatedBloodCompartment pulmonaryVeins;
        public AnimatedBloodConnector PVLA;
        public AnimatedBloodCompartment leftAtrium;
        public AnimatedValve mitralValve;
        public AnimatedBloodCompartment leftVentricle;
        public AnimatedValve aorticValve;
        public AnimatedBloodCompartment ascendingAorta;
        public AnimatedBloodCompartment descendingAorta;
        public AnimatedBloodConnector aorta;
        public AnimatedBloodCompartment lowerBody;
        public AnimatedBloodConnector LBIVC;
        public AnimatedBloodCompartment IVC;
        public AnimatedBloodConnector IVCRA;
        public AnimatedBloodCompartment rightAtrium;
        public AnimatedValve tricuspidValve;
        public AnimatedBloodCompartment rightVentricle;
        public AnimatedValve pulmonaryValve;
        public AnimatedBloodCompartment pulmonaryArtery;
        public AnimatedBloodConnector PALUNG;
        public AnimatedBloodCompartment ALBLOOD;
        public AnimatedBloodConnector ADALBLOOD;
        public AnimatedBloodConnector ALBLOODIVC;

        public AnimatedBloodConnector AAUB;
        public AnimatedBloodCompartment upperBody;
        public AnimatedBloodConnector UBSVC;
        public AnimatedBloodCompartment SVC;
        public AnimatedBloodConnector SVCRA;

        public AnimatedShunt PDA;
        public AnimatedShunt VSD;
        public AnimatedShunt OFO;
        public AnimatedShunt LUNGSHUNT;

        public AnimatedShuntGas OUTNCA;
        public AnimatedBloodCompartment lungs;
        public AnimatedGasComp alveoli;
        public AnimatedGasComp placenta;

        public AnimatedBloodCompartment lvad;
        public AnimatedBloodCompartment rvad;
        public AnimatedBloodCompartment ecmopump;
        public AnimatedBloodCompartment ecmolungblood;
        public AnimatedGasComp ecmolunggas;

        readonly SKPaint paint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = false,
            Color = SKColors.DarkGray,
            StrokeWidth = 10
        };
        readonly SKPaint airwayPaint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            Color = SKColors.DarkGray,
            StrokeWidth = 25,
        };
        readonly SKPaint airwayBronchi = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            Color = SKColors.DarkGray,
            StrokeWidth = 25,
        };

        SKSurface MainSurface;
        SKSurface SkeletonSurface;

        SKCanvas MainCanvas;
        SKCanvas SkeletonCanvas;

        bool initialized = false;

        public ModelDiagram()
        {
            InitializeComponent();
        }

        public void InitModelDiagram(Model cm)
        {
            currentModel = cm;

            initialized = true;
        }

        public void UpdateSkeleton()
        {
            canvasSkeleton.InvalidateVisual();
        }
        public void UpdatedMainDiagram()
        {
            canvasMain.InvalidateVisual();

        }
        public void ClearLists()
        {
            animatedBloodCompartments.Clear();
            animatedGasCompartments.Clear();
            animatedBloodConnectors.Clear();
            animatedGasConnectors.Clear();
            animatedValves.Clear();
            animatedShunts.Clear();
            animatedShuntsGas.Clear();

        }
        public void BuildDiagram()
        {

            ClearLists();

     

            pulmonaryValve = new AnimatedValve
            {
                scaleRelative = 0.035f,
                Degrees = 0,
                NoLoss = false,
                RadiusYOffset = 1f,
                StartAngle = 190,
                EndAngle = 220,
                Direction = 1,
                Name = "PV"
            };
            pulmonaryValve.AddConnector(currentModel.modelState.RV_PA);
            animatedValves.Add(pulmonaryValve);

            pulmonaryArtery = new AnimatedBloodCompartment
            {
                scaleRelative = 0.035f,
                IsVessel = true,
                StartAngle = 220,
                EndAngle = 230,
                Degrees = 225,
                Name = "PA"
            };
            pulmonaryArtery.AddCompartment(currentModel.modelState.PA);
            animatedBloodCompartments.Add(pulmonaryArtery);

            PALUNG = new AnimatedBloodConnector
            {
                scaleRelative = 0.035f,
                RadiusYOffset = 1f,
                NoLoss = true,
                StartAngle = 230,
                EndAngle = 270,
                Direction = 1,
                Name = "PA_LUNG"
            };
            PALUNG.AddConnector(currentModel.modelState.PA_LL);
            PALUNG.AddConnector(currentModel.modelState.PA_LR);
            animatedBloodConnectors.Add(PALUNG);

            aorticValve = new AnimatedValve
            {
                scaleRelative = 0.035f,
                Degrees = 0,
                NoLoss = false,
                RadiusYOffset = 1f,
                StartAngle = 0,
                EndAngle = 15,
                Direction = 1,
                Name = "AV"
            };
            aorticValve.AddConnector(currentModel.modelState.LV_AA);
            animatedValves.Add(aorticValve);

            ascendingAorta = new AnimatedBloodCompartment
            {
                scaleRelative = 0.035f,
                IsVessel = true,
                RadiusYOffset = 1f,
                StartAngle = 25,
                EndAngle = 15,
                Degrees = 20,
                OffsetXFactor = 0.75f,
                Direction = -1,
                Name = "AA"
            };
            ascendingAorta.AddCompartment(currentModel.modelState.AA);
            animatedBloodCompartments.Add(ascendingAorta);

            descendingAorta = new AnimatedBloodCompartment
            {
                scaleRelative = 0.035f,
                IsVessel = true,
                RadiusYOffset = 1f,
                StartAngle = 35,
                EndAngle = 25,
                Degrees = 30,
                Direction = -1,
                Name = "AD"
            };
            descendingAorta.AddCompartment(currentModel.modelState.AD);
            animatedBloodCompartments.Add(descendingAorta);

            aorta = new AnimatedBloodConnector
            {
                scaleRelative = 0.035f,
                Degrees = 0,
                NoLoss = true,
                RadiusYOffset = 1f,
                StartAngle = 35,
                EndAngle = 90,
                Direction = 1,
                Name = "AD->LB"
            };
            aorta.AddConnector(currentModel.modelState.AD_KIDNEYS);
            aorta.AddConnector(currentModel.modelState.AD_LB);
            aorta.AddConnector(currentModel.modelState.AD_LIVER);
            animatedBloodConnectors.Add(aorta);

            myocardium = new AnimatedBloodCompartment
            {
                scaleRelative = 0.035f,
                Degrees = 90,
                RadiusYOffset = 0.725f,
                Name = "MYO"
            };
            myocardium.AddCompartment(currentModel.modelState.MYO);

            leftVentricle = new AnimatedBloodCompartment
            {
                scaleRelative = 0.035f,
                Degrees = 0,

                Name = "LV"
            };
            leftVentricle.AddCompartment(currentModel.modelState.LV);
            animatedBloodCompartments.Add(leftVentricle);



            leftAtrium = new AnimatedBloodCompartment
            {
                scaleRelative = 0.035f,
                Degrees = 340,
                Name = "LA"
            };
            leftAtrium.AddCompartment(currentModel.modelState.LA);
            animatedBloodCompartments.Add(leftAtrium);

            mitralValve = new AnimatedValve
            {
                scaleRelative = 0.035f,
                Degrees = 0,
                RadiusYOffset = 1f,
                StartAngle = 340,
                EndAngle = 360,
                Direction = 1,
                Name = "TV"
            };
            mitralValve.AddConnector(currentModel.modelState.LA_LV);
            animatedValves.Add(mitralValve);

            rightVentricle = new AnimatedBloodCompartment
            {
                scaleRelative = 0.035f,
                Degrees = 190,
                Name = "RV"
            };
            rightVentricle.AddCompartment(currentModel.modelState.RV);
            animatedBloodCompartments.Add(rightVentricle);

            rightAtrium = new AnimatedBloodCompartment
            {
                scaleRelative = 0.035f,
                Degrees = 170,
                Name = "RA"
            };
            rightAtrium.AddCompartment(currentModel.modelState.RA);
            animatedBloodCompartments.Add(rightAtrium);


            tricuspidValve = new AnimatedValve
            {
                scaleRelative = 0.035f,
                Degrees = 0,
                RadiusYOffset = 1f,
                StartAngle = 170,
                EndAngle = 190,
                Direction = 1,
                Name = "TV"
            };
            tricuspidValve.AddConnector(currentModel.modelState.RA_RV);
            animatedValves.Add(tricuspidValve);

            lowerBody = new AnimatedBloodCompartment
            {
                scaleRelative = 0.035f,
                Degrees = 90,
                Name = "LB"
            };
            lowerBody.AddCompartment(currentModel.modelState.LIVER);
            lowerBody.AddCompartment(currentModel.modelState.KIDNEYS);
            lowerBody.AddCompartment(currentModel.modelState.LB);
            animatedBloodCompartments.Add(lowerBody);

            lungs = new AnimatedBloodCompartment
            {
                scaleRelative = 0.035f,
                Degrees = 270,
                Name = "LUNG"
            };
            lungs.AddCompartment(currentModel.modelState.LL);
            lungs.AddCompartment(currentModel.modelState.LR);
            animatedBloodCompartments.Add(lungs);

            upperBody = new AnimatedBloodCompartment
            {
                scaleRelative = 0.035f,
                Degrees = 90,
                RadiusYOffset = 0.5f,
                Name = "UB"
            };
            upperBody.AddCompartment(currentModel.modelState.BRAIN);
            upperBody.AddCompartment(currentModel.modelState.UB);
            animatedBloodCompartments.Add(upperBody);

            alveoli = new AnimatedGasComp
            {
                scaleRelative = 0.035f,
                Degrees = 270,
                RadiusYOffset = 1f,
                Name = "ALV"
            };
            alveoli.AddCompartment(currentModel.modelState.ALL);
            alveoli.AddCompartment(currentModel.modelState.ALR);
            animatedGasCompartments.Add(alveoli);

        

         

            LBIVC = new AnimatedBloodConnector
            {
                scaleRelative = 0.035f,
                Degrees = 0,
                NoLoss = false,
                RadiusYOffset = 1f,
                StartAngle = 90,
                EndAngle = 120,
                Direction = 1,
                Name = "LB->IVC"
            };
            LBIVC.AddConnector(currentModel.modelState.LB_IVC);
            LBIVC.AddConnector(currentModel.modelState.KIDNEYS_IVC);
            LBIVC.AddConnector(currentModel.modelState.LIVER_IVC);
            animatedBloodConnectors.Add(LBIVC);

            IVC = new AnimatedBloodCompartment
            {
                scaleRelative = 0.035f,
                IsVessel = true,
                StartAngle = 130,
                EndAngle = 120,
                Degrees = 125,
                OffsetXFactor = 4f,
                Direction = -1,
                Name = "IVC"
            };
            IVC.AddCompartment(currentModel.modelState.IVC);
            animatedBloodCompartments.Add(IVC);

            IVCRA = new AnimatedBloodConnector
            {
                scaleRelative = 0.035f,
                RadiusYOffset = 1f,
                NoLoss = true,
                StartAngle = 130,
                EndAngle = 170,
                Direction = 1,
                Name = "IVC->RA"
            };
            IVCRA.AddConnector(currentModel.modelState.IVC_RA);
            animatedBloodConnectors.Add(IVCRA);

            UBSVC = new AnimatedBloodConnector
            {
                scaleRelative = 0.035f,
                Degrees = 0,
                NoLoss = false,
                RadiusYOffset = 0.5f,
                StartAngle = 90,
                EndAngle = 120,
                Direction = 1,
                Name = "UB->SVC"
            };
            UBSVC.AddConnector(currentModel.modelState.UB_SVC);
            UBSVC.AddConnector(currentModel.modelState.BRAIN_SVC);
            animatedBloodConnectors.Add(UBSVC);

            SVC = new AnimatedBloodCompartment
            {
                scaleRelative = 0.035f,
                IsVessel = true,
                RadiusYOffset = 0.5f,
                StartAngle = 130,
                OffsetXFactor = 5f,
                EndAngle = 120,
                Direction = -1,
                Degrees = 125,
                Name = "SVC"
            };
            SVC.AddCompartment(currentModel.modelState.SVC);
            animatedBloodCompartments.Add(SVC);

            SVCRA = new AnimatedBloodConnector
            {
                scaleRelative = 0.035f,
                RadiusYOffset = 0.5f,
                RadiusXOffset = 1.05f,
                NoLoss = true,
                StartAngle = 130,
                EndAngle = 165,
                Direction = 1,
                Name = "SVC->RA"
            };
            SVCRA.AddConnector(currentModel.modelState.SVC_RA);
            animatedBloodConnectors.Add(SVCRA);


            LUNGPV = new AnimatedBloodConnector
            {
                scaleRelative = 0.035f,
                RadiusYOffset = 1f,
                NoLoss = false,
                StartAngle = 270,
                EndAngle = 310,
                Direction = 1,
                Name = "LUNG->PV"
            };
            LUNGPV.AddConnector(currentModel.modelState.LL_PV);
            LUNGPV.AddConnector(currentModel.modelState.LR_PV);
            animatedBloodConnectors.Add(LUNGPV);

            pulmonaryVeins = new AnimatedBloodCompartment
            {
                scaleRelative = 0.035f,
                IsVessel = true,
                StartAngle = 310,
                EndAngle = 320,
                Degrees = 315,
                Name = "PV"
            };
            pulmonaryVeins.AddCompartment(currentModel.modelState.PV);
            animatedBloodCompartments.Add(pulmonaryVeins);

            PVLA = new AnimatedBloodConnector
            {
                scaleRelative = 0.035f,
                RadiusYOffset = 1f,
                StartAngle = 320,
                EndAngle = 340,
                Direction = 1,
                Name = "PV->LA"
            };
            PVLA.AddConnector(currentModel.modelState.PV_LA);
            animatedBloodConnectors.Add(PVLA);

            AAUB = new AnimatedBloodConnector
            {
                scaleRelative = 0.035f,
                RadiusYOffset = 0.5f,
                RadiusXOffset = 1.3f,
                NoLoss = true,
                StartAngle = 40,
                EndAngle = 90,
                Direction = 1,
                Name = "AA->UB"
            };
            AAUB.AddConnector(currentModel.modelState.AA_UB);
            AAUB.AddConnector(currentModel.modelState.AA_BRAIN);
            animatedBloodConnectors.Add(AAUB);


            PDA = new AnimatedShunt
            {
                scaleRelative = 0.035f,
                RadiusYOffset = 1f,
                NoLoss = false,
                StartAngle = 25,
                EndAngle = 225,
                Direction = 1,
                Name = "DUCTUS ARTERIOSUS"
            };
            PDA.AddConnector(currentModel.modelState.DA_PA);
            PDA.AddConnector(currentModel.modelState.AD_DA);


            VSD = new AnimatedShunt
            {
                scaleRelative = 0.035f,
                RadiusYOffset = 1f,
                NoLoss = true,
                StartAngle = 0,
                EndAngle = 190,
                Direction = 1,
                Name = "VSD"
            };
            VSD.AddConnector(currentModel.modelState.LV_RV);


            OFO = new AnimatedShunt
            {
                scaleRelative = 0.035f,
                RadiusYOffset = 1f,
                NoLoss = true,
                StartAngle = 340,
                EndAngle = 170,
                Direction = 1,
                Name = "FORAMEN OVALE"
            };
            OFO.AddConnector(currentModel.modelState.LA_RA);


            LUNGSHUNT = new AnimatedShunt
            {
                scaleRelative = 0.035f,
                RadiusYOffset = 1f,
                NoLoss = true,
                StartAngle = 315,
                EndAngle = 225,
                Direction = 1,
                Name = "LUNG SHUNT"
            };
            LUNGSHUNT.AddConnector(currentModel.modelState.PA_PV);


            OUTNCA = new AnimatedShuntGas
            {
                scaleRelative = 0.035f,
                RadiusYOffset = 1.45f,
                RadiusXOffset = 0f,
                NoLoss = true,
                StartAngle = 270,
                EndAngle = 230,
                Direction = 1,
                Name = "AIRWAY"
            };
            OUTNCA.AddConnector(currentModel.modelState.OUT_NCA);
            //animatedShuntsGas.Add(OUTNCA);

            AAMYO = new AnimatedBloodConnector
            {
                scaleRelative = 0.035f,
                Degrees = 0,
                NoLoss = true,
                RadiusYOffset = 0.725f,
                StartAngle = 20,
                EndAngle = 90,
                Direction = 1,
                Name = "AA->MYO"
            };
            AAMYO.AddConnector(currentModel.modelState.AA_MYO);

            MYORA = new AnimatedBloodConnector
            {
                scaleRelative = 0.035f,
                Degrees = 0,
                NoLoss = true,
                RadiusYOffset = 0.725f,
                StartAngle = 90,
                EndAngle = 170,
                Direction = 1,
                Name = "MYO->RA"
            };
            MYORA.AddConnector(currentModel.modelState.MYO_RA);

      
        }
        public void DrawMainDiagram(SKCanvas _canvas, float _width, float _height)
        {
            _canvas.Clear(SKColors.Transparent);
            _canvas.Translate(_width / 2, _height / 2);

            foreach (AnimatedBloodConnector ac in animatedBloodConnectors)
            {
                if (ac.IsVisible)
                    ac.DrawConnector(_canvas, _width, _height);
            }

            foreach (AnimatedGasConnector ac in animatedGasConnectors)
            {
                if (ac.IsVisible)
                    ac.DrawConnector(_canvas, _width, _height);
            }

            foreach (AnimatedValve av in animatedValves)
            {
                if (av.IsVisible)
                    av.DrawConnector(_canvas, _width, _height);
            }

            foreach (AnimatedShunt ash in animatedShunts)
            {
                if (ash.IsVisible)
                    ash.DrawConnector(_canvas, _width, _height);
            }

            foreach (AnimatedShuntGas ashg in animatedShuntsGas)
            {
                if (ashg.IsVisible)
                    ashg.DrawConnector(_canvas, _width, _height);
            }

            foreach (AnimatedGasComp ag in animatedGasCompartments)
            {
                if (ag.IsVisible)
                    ag.DrawCompartment(_canvas, _width, _height);
            }

            foreach (AnimatedBloodCompartment ab in animatedBloodCompartments)
            {
                if (ab.IsVisible)
                    ab.DrawCompartment(_canvas, _width, _height);
            }
        }
        public void DrawDiagramSkeleton(SKCanvas _canvas, float _width, float _height)
        {
            _canvas.Clear(SKColors.Transparent);
            _canvas.Translate(_width / 2, _height / 2);

            // draw main circle
            SKPoint location = new SKPoint(0, 0);
            float radius = _width / 2.5f;
            if (_width > _height)
            {
                radius = _height / 2.5f;
            }

            _canvas.DrawCircle(location, radius, paint);

        }


        public void PDAView(bool state)
        {
            if (state)
            {
                if (!animatedShunts.Contains(PDA))
                    animatedShunts.Add(PDA);
            }
            else
            {
                if (animatedShunts.Contains(PDA))
                    animatedShunts.Remove(PDA);
            }
        }
        public void VSDView(bool state)
        {
            if (state)
            {
                if (!animatedShunts.Contains(VSD))
                    animatedShunts.Add(VSD);
            }
            else
            {
                if (animatedShunts.Contains(VSD))
                    animatedShunts.Remove(VSD);
            }
        }
        public void OFOView(bool state)
        {
            if (state)
            {
                if (!animatedShunts.Contains(OFO))
                    animatedShunts.Add(OFO);
            }
            else
            {
                if (animatedShunts.Contains(OFO))
                    animatedShunts.Remove(OFO);
            }
        }
        public void LUNGSHUNTView(bool state)
        {
            if (state)
            {
                if (!animatedShunts.Contains(LUNGSHUNT))
                    animatedShunts.Add(LUNGSHUNT);
            }
            else
            {
                if (animatedShunts.Contains(LUNGSHUNT))
                    animatedShunts.Remove(LUNGSHUNT);
            }
        }

        public void MYOView(bool state)
        {
            if (state)
            {
                if (!animatedBloodCompartments.Contains(myocardium))
                    animatedBloodCompartments.Add(myocardium);
                if (!animatedBloodConnectors.Contains(MYORA))
                    animatedBloodConnectors.Insert(0, MYORA);
                if (!animatedBloodConnectors.Contains(AAMYO))
                    animatedBloodConnectors.Insert(1, AAMYO);
            }
            else
            {
                animatedBloodCompartments.Remove(myocardium);
                animatedBloodConnectors.Remove(AAMYO);
                animatedBloodConnectors.Remove(MYORA);
            }
        }

        private void CanvasSkeleton_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            SkeletonSurface = e.Surface;
            SkeletonCanvas = SkeletonSurface.Canvas;

            if (initialized)
            {
                DrawDiagramSkeleton(SkeletonCanvas, canvasSkeleton.CanvasSize.Width, canvasSkeleton.CanvasSize.Height);
            }
        }

        private void CanvasMain_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            MainSurface = e.Surface;
            MainCanvas = MainSurface.Canvas;

            if (initialized)
            {
                DrawMainDiagram(MainCanvas, canvasMain.CanvasSize.Width, canvasMain.CanvasSize.Height);
            }
        }

        private void SwitchOFO_Checked(object sender, RoutedEventArgs e)
        {
            OFOView(true);
        }

        private void SwitchOFO_Unchecked(object sender, RoutedEventArgs e)
        {
            OFOView(false);
        }

        private void SwitchPDA_Checked(object sender, RoutedEventArgs e)
        {
            PDAView(true);
        }

        private void SwitchPDA_Checked_1(object sender, RoutedEventArgs e)
        {
            PDAView(true);
        }

        private void SwitchPDA_Unchecked(object sender, RoutedEventArgs e)
        {
            PDAView(false);
        }

        private void SwitchVSD_Checked(object sender, RoutedEventArgs e)
        {
            VSDView(true);
        }

        private void SwitchVSD_Unchecked(object sender, RoutedEventArgs e)
        {
            VSDView(false);
        }

        private void SwitchLUNG_Checked(object sender, RoutedEventArgs e)
        {
            LUNGSHUNTView(true);
        }

        private void SwitchLUNG_Unchecked(object sender, RoutedEventArgs e)
        {
            LUNGSHUNTView(false);
        }
    }
}
