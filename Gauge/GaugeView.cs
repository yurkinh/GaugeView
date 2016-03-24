using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Java.Lang;
using Android.Text;
using Math = Java.Lang.Math;

namespace Gauge
{
  public class GaugeView : View
  {
    #region const fields
    public const int SIZE = 300;
    public const float TOP = 0.0f;
    public const float LEFT = 0.0f;
    public const float RIGHT = 1.0f;
    public const float BOTTOM = 1.0f;
    public const float CENTER = 0.5f;
    public const bool SHOW_OUTER_SHADOW = true;
    public const bool SHOW_OUTER_BORDER = true;
    public const bool SHOW_OUTER_RIM = true;
    public const bool SHOW_INNER_RIM = true;
    public const bool SHOW_NEEDLE = true;
    public const bool SHOW_SCALE = false;
    public const bool SHOW_RANGES = true;
    public const bool SHOW_TEXT = false;

    public const float OUTER_SHADOW_WIDTH = 0.03f;
    public const float OUTER_BORDER_WIDTH = 0.04f;
    public const float OUTER_RIM_WIDTH = 0.05f;
    public const float INNER_RIM_WIDTH = 0.06f;
    public const float INNER_RIM_BORDER_WIDTH = 0.005f;

    public const float NEEDLE_WIDTH = 0.035f;
    public const float NEEDLE_HEIGHT = 0.28f;

    public const float SCALE_POSITION = 0.025f;
    public const float SCALE_START_VALUE = 0.0f;
    public const float SCALE_END_VALUE = 100.0f;
    public const float SCALE_START_ANGLE = 30.0f;
    public const int SCALE_DIVISIONS = 10;
    public const int SCALE_SUBDIVISIONS = 5;

    public readonly int[] OUTER_SHADOW_COLORS = { Color.Argb( 40, 255, 254, 187 ), Color.Argb( 20, 255, 247, 219 ), Color.Argb( 5, 255, 255, 255 ) };
    public readonly float[] OUTER_SHADOW_POS = { 0.90f, 0.95f, 0.99f };

    public readonly float[] RANGE_VALUES = { 16.0f, 25.0f, 40.0f, 100.0f };
    public readonly int[] RANGE_COLORS = { Color.Rgb( 231, 32, 43 ), Color.Rgb( 232, 111, 33 ), Color.Rgb( 232, 231, 33 ), Color.Rgb( 27, 202, 33 ) };

    public readonly int TEXT_SHADOW_COLOR = Color.Argb( 100, 0, 0, 0 );
    public readonly int TEXT_VALUE_COLOR = Color.White;
    public readonly int TEXT_UNIT_COLOR = Color.White;
    public const float TEXT_VALUE_SIZE = 0.3f;
    public const float TEXT_UNIT_SIZE = 0.1f;
    #endregion    

    #region Customizable fields
    private bool mShowOuterShadow;
    private bool mShowOuterBorder;
    private bool mShowOuterRim;
    private bool mShowInnerRim;
    private bool mShowScale;
    private bool mShowRanges;
    private bool mShowNeedle;
    private bool mShowText;

    private float mOuterShadowWidth;
    private float mOuterBorderWidth;
    private float mOuterRimWidth;
    private float mInnerRimWidth;
    private float mInnerRimBorderWidth;
    private float mNeedleWidth;
    private float mNeedleHeight;

    private float mScalePosition;
    private float mScaleStartValue;
    private float mScaleEndValue;
    private float mScaleStartAngle;
    private float mScaleEndAngle;
    private float[] mRangeValues;

    private int[] mRangeColors;
    private int mDivisions;
    private int mSubdivisions;

    private RectF mOuterShadowRect;
    private RectF mOuterBorderRect;
    private RectF mOuterRimRect;
    private RectF mInnerRimRect;
    private RectF mInnerRimBorderRect;
    private RectF mFaceRect;
    private RectF mScaleRect;

    private Bitmap mBackground;
    private Paint mBackgroundPaint;
    private Paint mOuterShadowPaint;
    private Paint mOuterBorderPaint;
    private Paint mOuterRimPaint;
    private Paint mInnerRimPaint;
    private Paint mInnerRimBorderLightPaint;
    private Paint mInnerRimBorderDarkPaint;
    private Paint mFacePaint;
    private Paint mFaceBorderPaint;
    private Paint mFaceShadowPaint;
    private Paint[] mRangePaints;
    private Paint mNeedleRightPaint;
    private Paint mNeedleLeftPaint;
    private Paint mNeedleScrewPaint;
    private Paint mNeedleScrewBorderPaint;
    private Paint mTextValuePaint;
    private Paint mTextUnitPaint;

    private string mTextValue;
    private string mTextUnit;
    private int mTextValueColor;
    private int mTextUnitColor;
    private int mTextShadowColor;
    private float mTextValueSize;
    private float mTextUnitSize;

    private Path mNeedleRightPath;
    private Path mNeedleLeftPath;

    // *--------------------------------------------------------------------- *//

    private float mScaleRotation;
    private float mDivisionValue;
    private float mSubdivisionValue;
    private float mSubdivisionAngle;

    private float mTargetValue;
    private float mCurrentValue;

    private float mNeedleVelocity;
    private float mNeedleAcceleration;
    private long mNeedleLastMoved = -1;
    private bool mNeedleInitialized;
    #endregion

    public GaugeView( Context context, IAttributeSet attrs, int defStyle ) : base( context, attrs, defStyle )
    {
      ReadAttrs( context, attrs, defStyle );
      Init();
    }

    public GaugeView( Context context, IAttributeSet attrs ) : this( context, attrs, 0 )
    {

    }

    public GaugeView( Context context ) : this( context, null, 0 )
    {

    }

    private void ReadAttrs( Context context, IAttributeSet attrs, int defStyle )
    {
      TypedArray typedArray = context.ObtainStyledAttributes( attrs, Resource.Styleable.GaugeView, defStyle, 0 );
      mShowOuterShadow = typedArray.GetBoolean( Resource.Styleable.GaugeView_showOuterShadow, SHOW_OUTER_SHADOW );
      mShowOuterBorder = typedArray.GetBoolean( Resource.Styleable.GaugeView_showOuterBorder, SHOW_OUTER_BORDER );
      mShowOuterRim = typedArray.GetBoolean( Resource.Styleable.GaugeView_showOuterRim, SHOW_OUTER_RIM );
      mShowInnerRim = typedArray.GetBoolean( Resource.Styleable.GaugeView_showInnerRim, SHOW_INNER_RIM );
      mShowNeedle = typedArray.GetBoolean( Resource.Styleable.GaugeView_showNeedle, SHOW_NEEDLE );
      mShowScale = typedArray.GetBoolean( Resource.Styleable.GaugeView_showScale, SHOW_SCALE );
      mShowRanges = typedArray.GetBoolean( Resource.Styleable.GaugeView_showRanges, SHOW_RANGES );
      mShowText = typedArray.GetBoolean( Resource.Styleable.GaugeView_showText, SHOW_TEXT );

      mOuterShadowWidth = mShowOuterShadow ? typedArray.GetFloat( Resource.Styleable.GaugeView_outerShadowWidth, OUTER_SHADOW_WIDTH ) : 0.0f;
      mOuterBorderWidth = mShowOuterBorder ? typedArray.GetFloat( Resource.Styleable.GaugeView_outerBorderWidth, OUTER_BORDER_WIDTH ) : 0.0f;
      mOuterRimWidth = mShowOuterRim ? typedArray.GetFloat( Resource.Styleable.GaugeView_outerRimWidth, OUTER_RIM_WIDTH ) : 0.0f;
      mInnerRimWidth = mShowInnerRim ? typedArray.GetFloat( Resource.Styleable.GaugeView_innerRimWidth, INNER_RIM_WIDTH ) : 0.0f;
      mInnerRimBorderWidth = mShowInnerRim ? typedArray.GetFloat( Resource.Styleable.GaugeView_innerRimBorderWidth, INNER_RIM_BORDER_WIDTH ) : 0.0f;

      mNeedleWidth = typedArray.GetFloat( Resource.Styleable.GaugeView_needleWidth, NEEDLE_WIDTH );
      mNeedleHeight = typedArray.GetFloat( Resource.Styleable.GaugeView_needleHeight, NEEDLE_HEIGHT );

      mScalePosition = ( mShowScale || mShowRanges ) ? typedArray.GetFloat( Resource.Styleable.GaugeView_scalePosition, SCALE_POSITION ) : 0.0f;
      mScaleStartValue = typedArray.GetFloat( Resource.Styleable.GaugeView_scaleStartValue, SCALE_START_VALUE );
      mScaleEndValue = typedArray.GetFloat( Resource.Styleable.GaugeView_scaleEndValue, SCALE_END_VALUE );
      mScaleStartAngle = typedArray.GetFloat( Resource.Styleable.GaugeView_scaleStartAngle, SCALE_START_ANGLE );
      mScaleEndAngle = typedArray.GetFloat( Resource.Styleable.GaugeView_scaleEndAngle, 360.0f - mScaleStartAngle );

      mDivisions = typedArray.GetInteger( Resource.Styleable.GaugeView_divisions, SCALE_DIVISIONS );
      mSubdivisions = typedArray.GetInteger( Resource.Styleable.GaugeView_subdivisions, SCALE_SUBDIVISIONS );

      if ( mShowRanges )
      {
        mTextShadowColor = typedArray.GetColor( Resource.Styleable.GaugeView_textShadowColor, TEXT_SHADOW_COLOR );

        string[] rangeValues = typedArray.GetTextArray( Resource.Styleable.GaugeView_rangeValues );
        string[] rangeColors = typedArray.GetTextArray( Resource.Styleable.GaugeView_rangeColors );
        ReadRanges( rangeValues, rangeColors );
      }

      if ( mShowText )
      {
        int textValueId = typedArray.GetResourceId( Resource.Styleable.GaugeView_textValue, 0 );
        string textValue = typedArray.GetString( Resource.Styleable.GaugeView_textValue );
        mTextValue = ( 0 < textValueId ) ? context.GetString( textValueId ) : ( null != textValue ) ? textValue : "";

        int textUnitId = typedArray.GetResourceId( Resource.Styleable.GaugeView_textUnit, 0 );
        string textUnit = typedArray.GetString( Resource.Styleable.GaugeView_textUnit );
        mTextUnit = ( 0 < textUnitId ) ? context.GetString( textUnitId ) : ( null != textUnit ) ? textUnit : "";
        mTextValueColor = typedArray.GetColor( Resource.Styleable.GaugeView_textValueColor, TEXT_VALUE_COLOR );
        mTextUnitColor = typedArray.GetColor( Resource.Styleable.GaugeView_textUnitColor, TEXT_UNIT_COLOR );
        mTextShadowColor = typedArray.GetColor( Resource.Styleable.GaugeView_textShadowColor, TEXT_SHADOW_COLOR );

        mTextValueSize = typedArray.GetFloat( Resource.Styleable.GaugeView_textValueSize, TEXT_VALUE_SIZE );
        mTextUnitSize = typedArray.GetFloat( Resource.Styleable.GaugeView_textUnitSize, TEXT_UNIT_SIZE );
      }

      typedArray.Recycle();
    }

    private void ReadRanges( string[] rangeValues, string[] rangeColors )
    {

      int rangeValuesLength;
      if ( rangeValues == null )
      {
        rangeValuesLength = RANGE_VALUES.Length;
      }
      else
      {
        rangeValuesLength = rangeValues.Length;
      }

      int rangeColorsLength;
      if ( rangeColors == null )
      {
        rangeColorsLength = RANGE_COLORS.Length;
      }
      else
      {
        rangeColorsLength = rangeColors.Length;
      }

      if ( rangeValuesLength != rangeColorsLength )
      {
        throw new IllegalArgumentException( "The ranges and colors arrays must have the same length." );
      }

      int length = rangeValuesLength;
      if ( rangeValues != null )
      {
        mRangeValues = new float[length];
        for ( int i = 0; i < length; i++ )
        {
          mRangeValues[i] = Float.ParseFloat( rangeValues[i].ToString() );
        } 
      }
      else
      {
        mRangeValues = RANGE_VALUES;
      }

      if ( rangeColors != null )
      {
        mRangeColors = new int[length];
        for ( int i = 0; i < length; i++ )
        {
          mRangeColors[i] = Color.ParseColor( rangeColors[i].ToString() );
        }
      }
      else
      {
        mRangeColors = RANGE_COLORS;
      }
    }

    private void Init()
    {
      // TODO Why isn't this working with HA layer?
      // The needle is not displayed although the onDraw() is being triggered by invalidate()
      // calls.      
      if ( Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb )
      {
        SetLayerType( LayerType.Software, null );
      }

      InitDrawingRects();
      InitDrawingTools();

      // Compute the scale properties
      if ( mShowRanges )
      {
        InitScale();
      }
    }

    public void InitDrawingRects()
    {
      // The drawing area is a rectangle of width 1 and height 1,
      // where (0,0) is the top left corner of the canvas.
      // Note that on Canvas X axis points to right, while the Y axis points downwards.
      mOuterShadowRect = new RectF( LEFT, TOP, RIGHT, BOTTOM );

      mOuterBorderRect = new RectF( mOuterShadowRect.Left + mOuterShadowWidth, mOuterShadowRect.Top + mOuterShadowWidth,
          mOuterShadowRect.Right - mOuterShadowWidth, mOuterShadowRect.Bottom - mOuterShadowWidth );

      mOuterRimRect = new RectF( mOuterBorderRect.Left + mOuterBorderWidth, mOuterBorderRect.Top + mOuterBorderWidth,
          mOuterBorderRect.Right - mOuterBorderWidth, mOuterBorderRect.Bottom - mOuterBorderWidth );

      mInnerRimRect = new RectF( mOuterRimRect.Left + mOuterRimWidth, mOuterRimRect.Top + mOuterRimWidth, mOuterRimRect.Right
          - mOuterRimWidth, mOuterRimRect.Bottom - mOuterRimWidth );

      mInnerRimBorderRect = new RectF( mInnerRimRect.Left + mInnerRimBorderWidth, mInnerRimRect.Top + mInnerRimBorderWidth,
          mInnerRimRect.Right - mInnerRimBorderWidth, mInnerRimRect.Bottom - mInnerRimBorderWidth );

      mFaceRect = new RectF( mInnerRimRect.Left + mInnerRimWidth, mInnerRimRect.Top + mInnerRimWidth,
          mInnerRimRect.Right - mInnerRimWidth, mInnerRimRect.Bottom - mInnerRimWidth );

      mScaleRect = new RectF( mFaceRect.Left + mScalePosition, mFaceRect.Top + mScalePosition, mFaceRect.Right - mScalePosition,
          mFaceRect.Bottom - mScalePosition );
    }

    private void InitDrawingTools()
    {
      mBackgroundPaint = new Paint()
      {
        FilterBitmap = true
      };

      if ( mShowOuterShadow )
      {
        mOuterShadowPaint = GetDefaultOuterShadowPaint();
      }
      if ( mShowOuterBorder )
      {
        mOuterBorderPaint = GetDefaultOuterBorderPaint();
      }
      if ( mShowOuterRim )
      {
        mOuterRimPaint = GetDefaultOuterRimPaint();
      }
      if ( mShowInnerRim )
      {
        mInnerRimPaint = GetDefaultInnerRimPaint();
        mInnerRimBorderLightPaint = GetDefaultInnerRimBorderLightPaint();
        mInnerRimBorderDarkPaint = GetDefaultInnerRimBorderDarkPaint();
      }
      if ( mShowRanges )
      {
        SetDefaultScaleRangePaints();
      }
      if ( mShowNeedle )
      {
        SetDefaultNeedlePaths();
        mNeedleLeftPaint = GetDefaultNeedleLeftPaint();
        mNeedleRightPaint = GetDefaultNeedleRightPaint();
        mNeedleScrewPaint = GetDefaultNeedleScrewPaint();
        mNeedleScrewBorderPaint = GetDefaultNeedleScrewBorderPaint();
      }
      if ( mShowText )
      {
        mTextValuePaint = GetDefaultTextValuePaint();
        mTextUnitPaint = GetDefaultTextUnitPaint();
      }

      mFacePaint = GetDefaultFacePaint();
      mFaceBorderPaint = GetDefaultFaceBorderPaint();
      mFaceShadowPaint = GetDefaultFaceShadowPaint();
    }

    public Paint GetDefaultOuterShadowPaint()
    {
      Paint paint = new Paint( PaintFlags.AntiAlias );
      paint.SetStyle( Paint.Style.Fill );
      paint.SetShader( new RadialGradient( CENTER, CENTER, mOuterShadowRect.Width() / 2.0f, OUTER_SHADOW_COLORS, OUTER_SHADOW_POS, Shader.TileMode.Mirror ) );
      return paint;
    }

    private Paint GetDefaultOuterBorderPaint()
    {
      Paint paint = new Paint( PaintFlags.AntiAlias );
      paint.SetStyle( Paint.Style.Fill );
      paint.Color = Color.Argb( 245, 0, 0, 0 );
      return paint;
    }

    public Paint GetDefaultOuterRimPaint()
    {
      // Use a linear gradient to create the 3D effect
      LinearGradient verticalGradient = new LinearGradient( mOuterRimRect.Left, mOuterRimRect.Top, mOuterRimRect.Left,
          mOuterRimRect.Bottom, Color.Rgb( 255, 255, 255 ), Color.Rgb( 84, 90, 100 ), Shader.TileMode.Repeat );

      // Use a Bitmap shader for the metallic style
      Bitmap bitmap = BitmapFactory.DecodeResource( Resources, Resource.Drawable.light_alu );
      BitmapShader aluminiumTile = new BitmapShader( bitmap, Shader.TileMode.Repeat, Shader.TileMode.Repeat );
      Matrix matrix = new Matrix();
      matrix.SetScale( 1.0f / bitmap.Width, 1.0f / bitmap.Height );
      aluminiumTile.SetLocalMatrix( matrix );

      Paint paint = new Paint( PaintFlags.AntiAlias );
      paint.SetShader( new ComposeShader( verticalGradient, aluminiumTile, PorterDuff.Mode.Multiply ) );
      paint.FilterBitmap = true;
      return paint;
    }

    private Paint GetDefaultInnerRimPaint()
    {
      Paint paint = new Paint( PaintFlags.AntiAlias );
      paint.SetShader( new LinearGradient( mInnerRimRect.Left, mInnerRimRect.Top, mInnerRimRect.Left, mInnerRimRect.Bottom, new int[]{
        Color.Argb(255, 68, 73, 80), Color.Argb(255, 91, 97, 105), Color.Argb(255, 178, 180, 183), Color.Argb(255, 188, 188, 190),
        Color.Argb(255, 84, 90, 100), Color.Argb(255, 137, 137, 137)}, new float[] { 0, 0.1f, 0.2f, 0.4f, 0.8f, 1 }, Shader.TileMode.Clamp ) );
      return paint;
    }

    private Paint GetDefaultInnerRimBorderLightPaint()
    {
      Paint paint = new Paint( PaintFlags.AntiAlias );
      paint.SetStyle( Paint.Style.Stroke );
      paint.Color = Color.Argb( 100, 255, 255, 255 );
      paint.StrokeWidth = 0.005f;
      return paint;
    }

    private Paint GetDefaultInnerRimBorderDarkPaint()
    {
      Paint paint = new Paint( PaintFlags.AntiAlias );
      paint.SetStyle( Paint.Style.Stroke );
      paint.Color = Color.Argb( 100, 81, 84, 89 );
      paint.StrokeWidth = 0.005f;
      return paint;
    }

    public Paint GetDefaultFacePaint()
    {
      Paint paint = new Paint( PaintFlags.AntiAlias );
      paint.SetShader( new RadialGradient( 0.5f, 0.5f, mFaceRect.Width() / 2, new int[]{Color.Rgb(50, 132, 206), Color.Rgb(36, 89, 162),
        Color.Rgb(27, 59, 131)}, new float[] { 0.5f, 0.96f, 0.99f }, Shader.TileMode.Mirror ) );
      return paint;
    }

    public Paint GetDefaultFaceBorderPaint()
    {
      Paint paint = new Paint( PaintFlags.AntiAlias );
      paint.SetStyle( Paint.Style.Stroke );
      paint.Color = Color.Argb( 100, 81, 84, 89 );
      paint.StrokeWidth = 0.005f;
      return paint;
    }

    public Paint GetDefaultFaceShadowPaint()
    {
      Paint paint = new Paint( PaintFlags.AntiAlias );
      paint.SetShader( new RadialGradient( 0.5f, 0.5f, mFaceRect.Width() / 2.0f, new int[]{Color.Argb(60, 40, 96, 170),
        Color.Argb(80, 15, 34, 98), Color.Argb(120, 0, 0, 0), Color.Argb(140, 0, 0, 0)},
          new float[] { 0.60f, 0.85f, 0.96f, 0.99f }, Shader.TileMode.Mirror ) );
      return paint;
    }

    public void SetDefaultNeedlePaths()
    {
      float x = 0.5f, y = 0.5f;
      mNeedleLeftPath = new Path();
      mNeedleLeftPath.MoveTo( x, y );
      mNeedleLeftPath.LineTo( x - mNeedleWidth, y );
      mNeedleLeftPath.LineTo( x, y - mNeedleHeight );
      mNeedleLeftPath.LineTo( x, y );
      mNeedleLeftPath.LineTo( x - mNeedleWidth, y );

      mNeedleRightPath = new Path();
      mNeedleRightPath.MoveTo( x, y );
      mNeedleRightPath.LineTo( x + mNeedleWidth, y );
      mNeedleRightPath.LineTo( x, y - mNeedleHeight );
      mNeedleRightPath.LineTo( x, y );
      mNeedleRightPath.LineTo( x + mNeedleWidth, y );
    }

    public Paint GetDefaultNeedleLeftPaint()
    {
      Paint paint = new Paint( PaintFlags.AntiAlias );
      paint.Color = Color.Rgb( 176, 10, 19 );
      return paint;
    }

    public Paint GetDefaultNeedleRightPaint()
    {
      Paint paint = new Paint( PaintFlags.AntiAlias );
      paint.Color = Color.Rgb( 252, 18, 30 );
      paint.SetShadowLayer( 0.01f, 0.005f, -0.005f, Color.Argb( 127, 0, 0, 0 ) );
      return paint;
    }

    public Paint GetDefaultNeedleScrewPaint()
    {
      Paint paint = new Paint( PaintFlags.AntiAlias );
      paint.SetShader( new RadialGradient( 0.5f, 0.5f, 0.07f, new int[] { Color.Rgb( 171, 171, 171 ), Color.White }, new float[] { 0.05f, 0.9f }, Shader.TileMode.Mirror ) );
      return paint;
    }

    public Paint GetDefaultNeedleScrewBorderPaint()
    {
      Paint paint = new Paint( PaintFlags.AntiAlias );
      paint.SetStyle( Paint.Style.Stroke );
      paint.Color = Color.Argb( 100, 81, 84, 89 );
      paint.StrokeWidth = 0.005f;
      return paint;
    }

    public void SetDefaultScaleRangePaints()
    {
      int length = mRangeValues.Length;
      mRangePaints = new Paint[length];
      for ( int i = 0; i < length; i++ )
      {
        mRangePaints[i] = new Paint( PaintFlags.LinearText | PaintFlags.AntiAlias );
        mRangePaints[i].Color = new Color( mRangeColors[i] );
        mRangePaints[i].SetStyle( Paint.Style.Stroke );
        mRangePaints[i].StrokeWidth = 0.005f;
        mRangePaints[i].TextSize = 0.05f;
        mRangePaints[i].SetTypeface( Typeface.SansSerif );
        mRangePaints[i].TextAlign = Paint.Align.Center;
        mRangePaints[i].SetShadowLayer( 0.005f, 0.002f, 0.002f, new Color( mTextShadowColor ) );
      }
    }

    public Paint GetDefaultTextValuePaint()
    {
      Paint paint = new Paint( PaintFlags.LinearText | PaintFlags.AntiAlias );
      paint.Color = new Color( mTextValueColor );
      paint.SetStyle( Paint.Style.FillAndStroke );
      paint.StrokeWidth = 0.005f;
      paint.TextSize = mTextValueSize;
      paint.TextAlign = Paint.Align.Center;
      paint.SetTypeface( Typeface.SansSerif );
      paint.SetShadowLayer( 0.01f, 0.002f, 0.002f, new Color( mTextShadowColor ) );
      return paint;
    }

    public Paint GetDefaultTextUnitPaint()
    {
      Paint paint = new Paint( PaintFlags.LinearText | PaintFlags.AntiAlias );
      paint.Color = new Color( mTextUnitColor );
      paint.SetStyle( Paint.Style.FillAndStroke );
      paint.StrokeWidth = 0.005f;
      paint.TextSize = mTextUnitSize;
      paint.TextAlign = Paint.Align.Center;
      paint.SetShadowLayer( 0.01f, 0.002f, 0.002f, new Color( mTextShadowColor ) );
      return paint;
    }

    private void InitScale()
    {
      mScaleRotation = ( mScaleStartAngle + 180 ) % 360;
      mDivisionValue = ( mScaleEndValue - mScaleStartValue ) / mDivisions;
      mSubdivisionValue = mDivisionValue / mSubdivisions;
      mSubdivisionAngle = ( mScaleEndAngle - mScaleStartAngle ) / ( mDivisions * mSubdivisions );
    }


    protected override void OnRestoreInstanceState( IParcelable state )
    {
      Bundle bundle = (Bundle)state;
      IParcelable superState = (IParcelable)bundle.GetParcelable( "superState" );
      base.OnRestoreInstanceState( superState );

      mNeedleInitialized = bundle.GetBoolean( "needleInitialized" );
      mNeedleVelocity = bundle.GetFloat( "needleVelocity" );
      mNeedleAcceleration = bundle.GetFloat( "needleAcceleration" );
      mNeedleLastMoved = bundle.GetLong( "needleLastMoved" );
      mCurrentValue = bundle.GetFloat( "currentValue" );
      mTargetValue = bundle.GetFloat( "targetValue" );
    }

    protected override IParcelable OnSaveInstanceState()
    {
      IParcelable superState = base.OnSaveInstanceState();
      Bundle state = new Bundle();
      state.PutParcelable( "superState", superState );
      state.PutBoolean( "needleInitialized", mNeedleInitialized );
      state.PutFloat( "needleVelocity", mNeedleVelocity );
      state.PutFloat( "needleAcceleration", mNeedleAcceleration );
      state.PutLong( "needleLastMoved", mNeedleLastMoved );
      state.PutFloat( "currentValue", mCurrentValue );
      state.PutFloat( "targetValue", mTargetValue );
      return state;
    }

    protected override void OnMeasure( int widthMeasureSpec, int heightMeasureSpec )
    {
      MeasureSpecMode widthMode = MeasureSpec.GetMode( widthMeasureSpec );
      MeasureSpecMode heightMode = MeasureSpec.GetMode( heightMeasureSpec );
      int widthSize = MeasureSpec.GetSize( widthMeasureSpec );
      int heightSize = MeasureSpec.GetSize( heightMeasureSpec );

      int chosenWidth = ChooseDimension( widthMode, widthSize );
      int chosenHeight = ChooseDimension( heightMode, heightSize );
      SetMeasuredDimension( chosenWidth, chosenHeight );
    }

    private int ChooseDimension( MeasureSpecMode mode, int size )
    {
      switch ( mode )
      {
        case MeasureSpecMode.AtMost:
        case MeasureSpecMode.Exactly:
          return size;
        case MeasureSpecMode.Unspecified:
        default:
          return GetDefaultDimension();
      }
    }

    private int GetDefaultDimension()
    {
      return SIZE;
    }

    protected override void OnSizeChanged( int w, int h, int oldw, int oldh )
    {
      base.OnSizeChanged( w, h, oldw, oldh );
      DrawGauge();
    }

    private void DrawGauge()
    {
      if ( null != mBackground )
      {
        // Let go of the old background
        mBackground.Recycle();
      }
      // Create a new background according to the new width and height
      mBackground = Bitmap.CreateBitmap( Width, Height, Bitmap.Config.Argb8888 );
      Canvas canvas = new Canvas( mBackground );
      float scale = Java.Lang.Math.Min( Width, Height );
      canvas.Scale( scale, scale );
      canvas.Translate( ( scale == Height ) ? ( ( Width - scale ) / 2 ) / scale : 0, ( scale == Width ) ? ( ( Height - scale ) / 2 ) / scale : 0 );

      DrawRim( canvas );
      DrawFace( canvas );

      if ( mShowRanges )
      {
        DrawScale( canvas );
      }
    }

    protected override void OnDraw( Canvas canvas )
    {
      DrawBackground( canvas );

      float scale = Java.Lang.Math.Min( Width, Height );
      canvas.Scale( scale, scale );
      canvas.Translate( ( scale == Height ) ? ( ( Width - scale ) / 2 ) / scale : 0, ( scale == Width ) ? ( ( Height - scale ) / 2 ) / scale : 0 );

      if ( mShowNeedle )
      {
        DrawNeedle( canvas );
      }

      if ( mShowText )
      {
        DrawText( canvas );
      }

      ComputeCurrentValue();
    }

    private void DrawBackground( Canvas canvas )
    {
      if ( null != mBackground )
      {
        canvas.DrawBitmap( mBackground, 0, 0, mBackgroundPaint );
      }
    }

    private void DrawRim( Canvas canvas )
    {
      if ( mShowOuterShadow )
      {
        canvas.DrawOval( mOuterShadowRect, mOuterShadowPaint );
      }
      if ( mShowOuterBorder )
      {
        canvas.DrawOval( mOuterBorderRect, mOuterBorderPaint );
      }
      if ( mShowOuterRim )
      {
        canvas.DrawOval( mOuterRimRect, mOuterRimPaint );
      }
      if ( mShowInnerRim )
      {
        canvas.DrawOval( mInnerRimRect, mInnerRimPaint );
        canvas.DrawOval( mInnerRimRect, mInnerRimBorderLightPaint );
        canvas.DrawOval( mInnerRimBorderRect, mInnerRimBorderDarkPaint );
      }
    }

    private void DrawFace( Canvas canvas )
    {
      // Draw the face gradient
      canvas.DrawOval( mFaceRect, mFacePaint );
      // Draw the face border
      canvas.DrawOval( mFaceRect, mFaceBorderPaint );
      // Draw the inner face shadow
      canvas.DrawOval( mFaceRect, mFaceShadowPaint );
    }

    private void DrawText( Canvas canvas )
    {

      string textValue = !TextUtils.IsEmpty( mTextValue ) ? mTextValue : ValueString( mCurrentValue );
      float textValueWidth = mTextValuePaint.MeasureText( textValue );
      float textUnitWidth = !TextUtils.IsEmpty( mTextUnit ) ? mTextUnitPaint.MeasureText( mTextUnit ) : 0;
      if ( textValueWidth > 1 )
      {
        textValueWidth = textValueWidth * 0.002f;
        textUnitWidth = textUnitWidth * 0.00065f;
      }
      else
      {
        textValueWidth = textValueWidth * 0.5f;
        textUnitWidth = textUnitWidth * 0.5f;
      }

      float startX = CENTER - textUnitWidth;
      float startY = CENTER + 0.1f;

      DrawTextOnCanvasWithMagnifier( canvas, textValue, startX, startY, mTextValuePaint );

      if ( !TextUtils.IsEmpty( mTextUnit ) )
      {
        DrawTextOnCanvasWithMagnifier( canvas, mTextUnit, CENTER + textValueWidth + 0.03f, CENTER, mTextUnitPaint );
      }
    }

    private void DrawScale( Canvas canvas )
    {
      canvas.Save( SaveFlags.Matrix );
      // On canvas, North is 0 degrees, East is 90 degrees, South is 180 etc.
      // We start the scale somewhere South-West so we need to first rotate the canvas.
      canvas.Rotate( mScaleRotation, 0.5f, 0.5f );

      int totalTicks = mDivisions * mSubdivisions + 1;
      for ( int i = 0; i < totalTicks; i++ )
      {
        float y1 = mScaleRect.Top;
        float y2 = y1 + 0.015f; // height of division
        float y3 = y1 + 0.045f; // height of subdivision

        float value = GetValueForTick( i );
        Paint paint = GetRangePaint( value );
        float mod = value % mDivisionValue;

        if ( ( Math.Abs( mod - 0 ) < 0.001 ) || ( Math.Abs( mod - mDivisionValue ) < 0.001 ) )
        {
          // Draw a division tick
          canvas.DrawLine( 0.5f, y1, 0.5f, y3, paint );
          // Draw the text 0.15 away from the division tick
          DrawTextOnCanvasWithMagnifier( canvas, ValueString( value ), 0.5f, y3 + 0.045f, paint );
        }
        else
        {
          // Draw a subdivision tick
          canvas.DrawLine( 0.5f, y1, 0.5f, y2, paint );
        }
        canvas.Rotate( mSubdivisionAngle, 0.5f, 0.5f );
      }
      canvas.Restore();
    }

    // Workaround to fix missing text on Lollipop and above,
    // and probably some rendering issues with Jelly Bean and above
    // Modified from http://stackoverflow.com/a/14989037/746068
    public static void DrawTextOnCanvasWithMagnifier( Canvas canvas, string text, float x, float y, Paint paint )
    {
      if ( Build.VERSION.SdkInt <= BuildVersionCodes.IceCreamSandwichMr1 )
      {
        //draw normally
        canvas.DrawText( text, x, y, paint );
      }
      else
      {
        //workaround
        float originalStrokeWidth = paint.StrokeWidth;
        float originalTextSize = paint.TextSize;
        float magnifier = 1000f;

        canvas.Save();
        canvas.Scale( 1f / magnifier, 1f / magnifier );

        paint.TextSize = originalTextSize * magnifier;
        paint.StrokeWidth = originalStrokeWidth * magnifier;
        canvas.DrawText( text, x * magnifier, y * magnifier, paint );
        canvas.Restore();

        paint.TextSize = originalTextSize;
        paint.StrokeWidth = originalStrokeWidth;
      }
    }

    private string ValueString( float value )
    {
      return Java.Lang.String.Format( "%d", (int)value );
    }

    private float GetValueForTick( int tick )
    {
      return mScaleStartValue + tick * ( mDivisionValue / mSubdivisions );
    }

    private Paint GetRangePaint( float value )
    {
      int length = mRangeValues.Length;
      for ( int i = 0; i < length - 1; i++ )
      {
        if ( value < mRangeValues[i] )
          return mRangePaints[i];
      }
      if ( value <= mRangeValues[length - 1] )
        return mRangePaints[length - 1];
      throw new IllegalArgumentException( "Value " + value + " out of range!" );
    }

    private void DrawNeedle( Canvas canvas )
    {
      if ( mNeedleInitialized )
      {
        float angle = GetAngleForValue( mCurrentValue );

        canvas.Save( SaveFlags.Matrix );
        canvas.Rotate( angle, 0.5f, 0.5f );

        SetNeedleShadowPosition( angle );
        canvas.DrawPath( mNeedleLeftPath, mNeedleLeftPaint );
        canvas.DrawPath( mNeedleRightPath, mNeedleRightPaint );

        canvas.Restore();

        // Draw the needle screw and its border
        canvas.DrawCircle( 0.5f, 0.5f, 0.04f, mNeedleScrewPaint );
        canvas.DrawCircle( 0.5f, 0.5f, 0.04f, mNeedleScrewBorderPaint );
      }
    }

    private void SetNeedleShadowPosition( float angle )
    {
      if ( angle > 180 && angle < 360 )
      {
        // Move shadow from right to left
        mNeedleRightPaint.SetShadowLayer( 0, 0, 0, Color.Black );
        mNeedleLeftPaint.SetShadowLayer( 0.01f, -0.005f, 0.005f, Color.Argb( 127, 0, 0, 0 ) );
      }
      else
      {
        // Move shadow from left to right
        mNeedleLeftPaint.SetShadowLayer( 0, 0, 0, Color.Black );
        mNeedleRightPaint.SetShadowLayer( 0.01f, 0.005f, -0.005f, Color.Argb( 127, 0, 0, 0 ) );
      }
    }

    private float GetAngleForValue( float value )
    {
      return ( mScaleRotation + ( ( value - mScaleStartValue ) / mSubdivisionValue ) * mSubdivisionAngle ) % 360;
    }

    private void ComputeCurrentValue()
    {
      if ( !( Math.Abs( mCurrentValue - mTargetValue ) > 0.01f ) )
      {
        return;
      }

      if ( -1 != mNeedleLastMoved )
      {
        float time = ( JavaSystem.CurrentTimeMillis() - mNeedleLastMoved ) / 1000.0f;
        float direction = Math.Signum( mNeedleVelocity );
        if ( Math.Abs( mNeedleVelocity ) < 90.0f )
        {
          mNeedleAcceleration = 5.0f * ( mTargetValue - mCurrentValue );
        }
        else
        {
          mNeedleAcceleration = 0.0f;
        }

        mNeedleAcceleration = 5.0f * ( mTargetValue - mCurrentValue );
        mCurrentValue += mNeedleVelocity * time;
        mNeedleVelocity += mNeedleAcceleration * time;

        if ( ( mTargetValue - mCurrentValue ) * direction < 0.01f * direction )
        {
          mCurrentValue = mTargetValue;
          mNeedleVelocity = 0.0f;
          mNeedleAcceleration = 0.0f;
          mNeedleLastMoved = -1L;
        }
        else
        {
          mNeedleLastMoved = JavaSystem.CurrentTimeMillis();
        }

        Invalidate();

      }
      else
      {
        mNeedleLastMoved = JavaSystem.CurrentTimeMillis();
        ComputeCurrentValue();
      }
    }

    public void SetTargetValue( float value )
    {
      if ( mShowScale || mShowRanges )
      {
        if ( value < mScaleStartValue )
        {
          mTargetValue = mScaleStartValue;
        }
        else if ( value > mScaleEndValue )
        {
          mTargetValue = mScaleEndValue;
        }
        else
        {
          mTargetValue = value;
        }
      }
      else
      {
        mTargetValue = value;
      }
      mNeedleInitialized = true;
      Invalidate();
    }
  }
}