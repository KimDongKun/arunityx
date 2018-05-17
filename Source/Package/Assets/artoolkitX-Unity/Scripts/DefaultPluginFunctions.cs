/*
 *  PluginFunctions.cs
 *  artoolkitX for Unity
 *
 *  This file is part of artoolkitX for Unity.
 *
 *  artoolkitX for Unity is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  artoolkitX for Unity is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with artoolkitX for Unity.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  As a special exception, the copyright holders of this library give you
 *  permission to link this library with independent modules to produce an
 *  executable, regardless of the license terms of these independent modules, and to
 *  copy and distribute the resulting executable under terms of your choice,
 *  provided that you also meet, for each linked independent module, the terms and
 *  conditions of the license of that module. An independent module is a module
 *  which is neither derived from nor based on this library. If you modify this
 *  library, you may extend this exception to your version of the library, but you
 *  are not obligated to do so. If you do not wish to do so, delete this exception
 *  statement from your version.
 *
 *  Copyright 2015 Daqri, LLC.
 *  Copyright 2010-2015 ARToolworks, Inc.
 *
 *  Author(s): Philip Lamb, Julian Looser, Thorsten Bux
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

public class DefaultPluginFunctions : IPluginFunctions
{
    [NonSerialized]
    private bool inited = false;

    // Delegate type declaration.
    public delegate void LogCallback([MarshalAs(UnmanagedType.LPStr)] string msg);

    // Delegate instance.
    private LogCallback logCallback = null;
    private GCHandle logCallbackGCH;

    private static int ARW_TRACKER_OPTION_NFT_MULTIMODE = 0,                          ///< bool.
                       ARW_TRACKER_OPTION_SQUARE_THRESHOLD = 1,                       ///< Threshold value used for image binarization. int in range [0-255].
                       ARW_TRACKER_OPTION_SQUARE_THRESHOLD_MODE = 2,                  ///< Threshold mode used for image binarization. int.
                       ARW_TRACKER_OPTION_SQUARE_LABELING_MODE = 3,                   ///< int.
                       ARW_TRACKER_OPTION_SQUARE_PATTERN_DETECTION_MODE = 4,          ///< int.
                       ARW_TRACKER_OPTION_SQUARE_BORDER_SIZE = 5,                     ///< float in range (0-0.5).
                       ARW_TRACKER_OPTION_SQUARE_MATRIX_CODE_TYPE = 6,                ///< int.
                       ARW_TRACKER_OPTION_SQUARE_IMAGE_PROC_MODE = 7,                 ///< int.
                       ARW_TRACKER_OPTION_SQUARE_DEBUG_MODE = 8,                      ///< Enables or disable state of debug mode in the tracker. When enabled, a black and white debug image is generated during marker detection. The debug image is useful for visualising the binarization process and choosing a threshold value. bool.
                       ARW_TRACKER_OPTION_SQUARE_PATTERN_SIZE = 9,                    ///< Number of rows and columns in square template (pattern) markers. Defaults to AR_PATT_SIZE1, which is 16 in all versions of ARToolKit prior to 5.3. int.
                       ARW_TRACKER_OPTION_SQUARE_PATTERN_COUNT_MAX = 10;

    bool IPluginFunctions.Inited
    {
        get
        {
            return this.inited;
        }

        set
        {
            this.inited = value;
        }
    }

    public void arwRegisterLogCallback(LogCallback lcb)
    {
        logCallback = lcb;
		if (lcb != null) {
            logCallbackGCH = GCHandle.Alloc(logCallback); // Does not need to be pinned, see http://stackoverflow.com/a/19866119/316487 
        }
        if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwRegisterLogCallback(logCallback);
        else ARNativePlugin.arwRegisterLogCallback(logCallback);
		if (lcb == null) {
            logCallbackGCH.Free();
        }
    }

    public void arwSetLogLevel(int logLevel)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetLogLevel(logLevel);
        else ARNativePlugin.arwSetLogLevel(logLevel);
    }

    public bool arwInitialiseAR(int pattSize = 16, int pattCountMax = 25)
    {
        bool ok;
        if (Application.platform == RuntimePlatform.IPhonePlayer) {
            ok = ARNativePluginStatic.arwInitialiseAR();
            arwSetPatternSize(pattSize);
            arwSetPatternCountMax(pattCountMax);
            //            ok = ARNativePluginStatic.arwInitialiseARWithOptions (pattSize, pattCountMax);
        } else {
            ok = ARNativePlugin.arwInitialiseAR();
            arwSetPatternSize(pattSize);
            arwSetPatternCountMax(pattCountMax);
        }
        if (ok) this.inited = true;
        return ok;
    }

    public void arwSetPatternSize(int size)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            ARNativePluginStatic.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_PATTERN_SIZE, size);
        else
            ARNativePlugin.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_PATTERN_SIZE, size);
    }

    public void arwSetPatternCountMax(int count)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            ARNativePluginStatic.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_PATTERN_COUNT_MAX, count);
        else
            ARNativePlugin.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_PATTERN_COUNT_MAX, count);
    }

    public string arwGetARToolKitVersion()
    {
        StringBuilder sb = new StringBuilder(128);
        bool ok;
        if (Application.platform == RuntimePlatform.IPhonePlayer) ok = ARNativePluginStatic.arwGetARToolKitVersion(sb, sb.Capacity);
        else ok = ARNativePlugin.arwGetARToolKitVersion(sb, sb.Capacity);
        if (ok) return sb.ToString();
        else return "unknown";
    }

    public int arwGetError()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetError();
        else return ARNativePlugin.arwGetError();
    }

    public bool arwShutdownAR()
    {
        bool ok;
        if (Application.platform == RuntimePlatform.IPhonePlayer) ok = ARNativePluginStatic.arwShutdownAR();
        else ok = ARNativePlugin.arwShutdownAR();
        if (ok) this.inited = false;
        return ok;
    }

    public bool arwStartRunningB(string vconf, byte[] cparaBuff, int cparaBuffLen)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwStartRunningB(vconf, cparaBuff, cparaBuffLen);
        else return ARNativePlugin.arwStartRunningB(vconf, cparaBuff, cparaBuffLen);
    }

    public bool arwStartRunningStereoB(string vconfL, byte[] cparaBuffL, int cparaBuffLenL, string vconfR, byte[] cparaBuffR, int cparaBuffLenR, byte[] transL2RBuff, int transL2RBuffLen)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwStartRunningStereoB(vconfL, cparaBuffL, cparaBuffLenL, vconfR, cparaBuffR, cparaBuffLenR, transL2RBuff, transL2RBuffLen);
        else return ARNativePlugin.arwStartRunningStereoB(vconfL, cparaBuffL, cparaBuffLenL, vconfR, cparaBuffR, cparaBuffLenR, transL2RBuff, transL2RBuffLen);
    }

    public bool arwIsRunning()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwIsRunning();
        else return ARNativePlugin.arwIsRunning();
    }

    public bool arwStopRunning()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwStopRunning();
        else return ARNativePlugin.arwStopRunning();
    }

    public bool arwGetProjectionMatrix(float nearPlane, float farPlane, float[] matrix)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetProjectionMatrix(nearPlane, farPlane, matrix);
        else return ARNativePlugin.arwGetProjectionMatrix(nearPlane, farPlane, matrix);
    }

    public bool arwGetProjectionMatrixStereo(float nearPlane, float farPlane, float[] matrixL, float[] matrixR)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetProjectionMatrixStereo(nearPlane, farPlane, matrixL, matrixR);
        else return ARNativePlugin.arwGetProjectionMatrixStereo(nearPlane, farPlane, matrixL, matrixR);
    }

    public bool arwGetVideoParams(out int width, out int height, out int pixelSize, out String pixelFormatString)
    {
        StringBuilder sb = new StringBuilder(128);
        bool ok;
        if (Application.platform == RuntimePlatform.IPhonePlayer) ok = ARNativePluginStatic.arwGetVideoParams(out width, out height, out pixelSize, sb, sb.Capacity);
        else ok = ARNativePlugin.arwGetVideoParams(out width, out height, out pixelSize, sb, sb.Capacity);
        if (!ok) pixelFormatString = "";
        else pixelFormatString = sb.ToString();
        return ok;
    }

    public bool arwGetVideoParamsStereo(out int widthL, out int heightL, out int pixelSizeL, out String pixelFormatL, out int widthR, out int heightR, out int pixelSizeR, out String pixelFormatR)
    {
        StringBuilder sbL = new StringBuilder(128);
        StringBuilder sbR = new StringBuilder(128);
        bool ok;
        if (Application.platform == RuntimePlatform.IPhonePlayer) ok = ARNativePluginStatic.arwGetVideoParamsStereo(out widthL, out heightL, out pixelSizeL, sbL, sbL.Capacity, out widthR, out heightR, out pixelSizeR, sbR, sbR.Capacity);
        else ok = ARNativePlugin.arwGetVideoParamsStereo(out widthL, out heightL, out pixelSizeL, sbL, sbL.Capacity, out widthR, out heightR, out pixelSizeR, sbR, sbR.Capacity);
		if (!ok) {
            pixelFormatL = "";
            pixelFormatR = "";
		} else {
            pixelFormatL = sbL.ToString();
            pixelFormatR = sbR.ToString();
        }
        return ok;
    }

    public bool arwCapture()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwCapture();
        else return ARNativePlugin.arwCapture();
    }

    public bool arwUpdateAR()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwUpdateAR();
        else return ARNativePlugin.arwUpdateAR();
    }
    public bool arwUpdateTexture32([In, Out]Color32[] colors32)
    {
        bool ok;
        GCHandle handle = GCHandle.Alloc(colors32, GCHandleType.Pinned);
        IntPtr address = handle.AddrOfPinnedObject();
        if (Application.platform == RuntimePlatform.IPhonePlayer) ok = ARNativePluginStatic.arwUpdateTexture32(address);
        else ok = ARNativePlugin.arwUpdateTexture32(address);
        handle.Free();
        return ok;
    }

    public bool arwUpdateTexture32Stereo([In, Out]Color32[] colors32L, [In, Out]Color32[] colors32R)
    {
        bool ok;
        GCHandle handle0 = GCHandle.Alloc(colors32L, GCHandleType.Pinned);
        GCHandle handle1 = GCHandle.Alloc(colors32R, GCHandleType.Pinned);
        IntPtr address0 = handle0.AddrOfPinnedObject();
        IntPtr address1 = handle1.AddrOfPinnedObject();
        if (Application.platform == RuntimePlatform.IPhonePlayer) ok = ARNativePluginStatic.arwUpdateTexture32Stereo(address0, address1);
        else ok = ARNativePlugin.arwUpdateTexture32Stereo(address0, address1);
        handle0.Free();
        handle1.Free();
        return ok;
    }

    public int arwGetTrackablePatternCount(int markerID)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackablePatternCount(markerID);
        else return ARNativePlugin.arwGetTrackablePatternCount(markerID);
    }

    public bool arwGetTrackablePatternConfig(int markerID, int patternID, float[] matrix, out float width, out float height, out int imageSizeX, out int imageSizeY)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackablePatternConfig(markerID, patternID, matrix, out width, out height, out imageSizeX, out imageSizeY);
        else return ARNativePlugin.arwGetTrackablePatternConfig(markerID, patternID, matrix, out width, out height, out imageSizeX, out imageSizeY);
    }

    public bool arwGetTrackablePatternImage(int markerID, int patternID, [In, Out]Color[] colors)
    {
        bool ok;
        if (Application.platform == RuntimePlatform.IPhonePlayer) ok = ARNativePluginStatic.arwGetTrackablePatternImage(markerID, patternID, colors);
        else ok = ARNativePlugin.arwGetTrackablePatternImage(markerID, patternID, colors);
        return ok;
    }

    public bool arwGetTrackableOptionBool(int markerID, int option)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackableOptionBool(markerID, option);
        else return ARNativePlugin.arwGetTrackableOptionBool(markerID, option);
    }

    public void arwSetTrackableOptionBool(int markerID, int option, bool value)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackableOptionBool(markerID, option, value);
        else ARNativePlugin.arwSetTrackableOptionBool(markerID, option, value);
    }

    public int arwGetTrackableOptionInt(int markerID, int option)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackableOptionInt(markerID, option);
        else return ARNativePlugin.arwGetTrackableOptionInt(markerID, option);
    }

    public void arwSetTrackableOptionInt(int markerID, int option, int value)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackableOptionInt(markerID, option, value);
        else ARNativePlugin.arwSetTrackableOptionInt(markerID, option, value);
    }

    public float arwGetTrackableOptionFloat(int markerID, int option)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackableOptionFloat(markerID, option);
        else return ARNativePlugin.arwGetTrackableOptionFloat(markerID, option);
    }

    public void arwSetTrackableOptionFloat(int markerID, int option, float value)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackableOptionFloat(markerID, option, value);
        else ARNativePlugin.arwSetTrackableOptionFloat(markerID, option, value);
    }

    public void arwSetVideoDebugMode(bool debug)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionBool(ARW_TRACKER_OPTION_SQUARE_DEBUG_MODE, debug);
        else ARNativePlugin.arwSetTrackerOptionBool(ARW_TRACKER_OPTION_SQUARE_DEBUG_MODE, debug);
    }

    public bool arwGetVideoDebugMode()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionBool(ARW_TRACKER_OPTION_SQUARE_DEBUG_MODE);
        else return ARNativePlugin.arwGetTrackerOptionBool(ARW_TRACKER_OPTION_SQUARE_DEBUG_MODE);
    }

    public void arwSetVideoThreshold(int threshold)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_THRESHOLD, threshold);
        else ARNativePlugin.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_THRESHOLD, threshold);
    }

    public int arwGetVideoThreshold()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_THRESHOLD);
        else return ARNativePlugin.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_THRESHOLD);
    }

    public void arwSetVideoThresholdMode(int mode)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_THRESHOLD_MODE, mode);
        else ARNativePlugin.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_THRESHOLD_MODE, mode);
    }

    public int arwGetVideoThresholdMode()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_THRESHOLD_MODE);
        else return ARNativePlugin.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_THRESHOLD_MODE);
    }

    public void arwSetLabelingMode(int mode)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_LABELING_MODE, mode);
        else ARNativePlugin.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_LABELING_MODE, mode);
    }

    public int arwGetLabelingMode()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_LABELING_MODE);
        else return ARNativePlugin.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_LABELING_MODE);
    }

    public void arwSetBorderSize(float size)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionFloat(ARW_TRACKER_OPTION_SQUARE_BORDER_SIZE, size);
        else ARNativePlugin.arwSetTrackerOptionFloat(ARW_TRACKER_OPTION_SQUARE_BORDER_SIZE, size);
    }

    public float arwGetBorderSize()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionFloat(ARW_TRACKER_OPTION_SQUARE_BORDER_SIZE);
        else return ARNativePlugin.arwGetTrackerOptionFloat(ARW_TRACKER_OPTION_SQUARE_BORDER_SIZE);
    }

    public void arwSetPatternDetectionMode(int mode)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_PATTERN_DETECTION_MODE, mode);
        else ARNativePlugin.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_PATTERN_DETECTION_MODE, mode);
    }

    public int arwGetPatternDetectionMode()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_PATTERN_DETECTION_MODE);
        else return ARNativePlugin.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_PATTERN_DETECTION_MODE);
    }

    public void arwSetMatrixCodeType(int type)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_MATRIX_CODE_TYPE, type);
        else ARNativePlugin.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_MATRIX_CODE_TYPE, type);
    }

    public int arwGetMatrixCodeType()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_MATRIX_CODE_TYPE);
        else return ARNativePlugin.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_MATRIX_CODE_TYPE);
    }

    public void arwSetImageProcMode(int mode)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_IMAGE_PROC_MODE, mode);
        else ARNativePlugin.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_IMAGE_PROC_MODE, mode);
    }

    public int arwGetImageProcMode()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_IMAGE_PROC_MODE);
        else return ARNativePlugin.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_IMAGE_PROC_MODE);
    }

    public void arwSetNFTMultiMode(bool on)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionBool(ARW_TRACKER_OPTION_NFT_MULTIMODE, on);
        else ARNativePlugin.arwSetTrackerOptionBool(ARW_TRACKER_OPTION_NFT_MULTIMODE, on);
    }

    public bool arwGetNFTMultiMode()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionBool(ARW_TRACKER_OPTION_NFT_MULTIMODE);
        else return ARNativePlugin.arwGetTrackerOptionBool(ARW_TRACKER_OPTION_NFT_MULTIMODE);
    }


    public int arwAddTrackable(string cfg)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwAddTrackable(cfg);
        else return ARNativePlugin.arwAddTrackable(cfg);
    }

    public bool arwRemoveTrackable(int markerID)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwRemoveTrackable(markerID);
        else return ARNativePlugin.arwRemoveTrackable(markerID);
    }

    public int arwRemoveAllTrackables()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwRemoveAllTrackables();
        else return ARNativePlugin.arwRemoveAllTrackables();
    }

    public bool arwQueryTrackableVisibilityAndTransformation(int markerID, float[] matrix)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwQueryTrackableVisibilityAndTransformation(markerID, matrix);
        else return ARNativePlugin.arwQueryTrackableVisibilityAndTransformation(markerID, matrix);
    }

    public bool arwQueryTrackableVisibilityAndTransformationStereo(int markerID, float[] matrixL, float[] matrixR)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwQueryTrackableVisibilityAndTransformationStereo(markerID, matrixL, matrixR);
        else return ARNativePlugin.arwQueryTrackableVisibilityAndTransformationStereo(markerID, matrixL, matrixR);
    }

    public bool arwLoadOpticalParams(string optical_param_name, byte[] optical_param_buff, int optical_param_buffLen, float projectionNearPlane, float projectionFarPlane, out float fovy_p, out float aspect_p, float[] m, float[] p)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwLoadOpticalParams(optical_param_name, optical_param_buff, optical_param_buffLen, projectionNearPlane, projectionFarPlane, out fovy_p, out aspect_p, m, p);
        else return ARNativePlugin.arwLoadOpticalParams(optical_param_name, optical_param_buff, optical_param_buffLen, projectionNearPlane, projectionFarPlane, out fovy_p, out aspect_p, m, p);
    }

}