using SlidingPuzzle;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using SlidingPuzzle.Droid;
using System.ComponentModel;
using Android.Content;

[assembly: ExportRenderer(typeof(SlidingPuzzleView), typeof(SlingPuzzleViewRenderer))]
namespace SlidingPuzzle.Droid
{
    class SlingPuzzleViewRenderer : ViewRenderer<SlidingPuzzleView, PuzzleCanvasView>
    {
        Context context;

        public SlingPuzzleViewRenderer(Context context) : base(context)
        {
            this.context = context;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<SlidingPuzzleView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                //var paintView = new PaintView(context);
                //paintView.SetInkColor(Element.InkColor.ToAndroid());
                //paintView.LineDrawn += PaintViewLineDrawn;
                //SetNativeControl(paintView);

                //MessagingCenter.Subscribe<SketchView>(this, "Clear", OnMessageClear);

                var puzzleCanvasView = new PuzzleCanvasView(context);
                SetNativeControl(puzzleCanvasView);
            }
        }

        //void OnMessageClear(SketchView sender)
        //{
        //    if (sender == Element)
        //        Control.Clear();
        //}

        //private void PaintViewLineDrawn(object sender, System.EventArgs e)
        //{
        //    ((ISketchController)Element)?.SendSketchUpdated();
        //}

        //protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    base.OnElementPropertyChanged(sender, e);

        //    if (e.PropertyName == SketchView.InkColorProperty.PropertyName)
        //    {
        //        Control.SetInkColor(Element.InkColor.ToAndroid());
        //    }
        //}

        //protected override void Dispose(bool disposinf)
        //{
        //    MessagingCenter.Unsubscribe<SketchView>(this, "Clear");
        //    Control.LineDrawn -= PaintViewLineDrawn;
        //}
    }
        
}
