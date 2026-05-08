using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using MVR.FileManagement;
using UnityEngine;
using UnityEngine.UI;

public class ImageLoaderThreaded : MonoBehaviour
{
	public delegate void ImageLoaderCallback(QueuedImage qi);

	public class QueuedImage
	{
		public bool isThumbnail;

		public string imgPath;

		public bool skipCache;

		public bool forceReload;

		public bool createMipMaps;

		public bool compress = true;

		public bool linear;

		public bool processed;

		public bool finished;

		public bool isNormalMap;

		public bool createAlphaFromGrayscale;

		public bool createNormalFromBump;

		public float bumpStrength = 1f;

		public bool invert;

		public bool setSize;

		public bool fillBackground;

		public int width;

		public int height;

		public byte[] raw;

		public bool hadError;

		public string errorText;

		public TextureFormat textureFormat;

		public Texture2D tex;

		public RawImage rawImageToLoad;

		public ImageLoaderCallback callback;

		public string cacheSignature
		{
			get
			{
				string text = imgPath;
				if (compress)
				{
					text += ":C";
				}
				if (linear)
				{
					text += ":L";
				}
				if (isNormalMap)
				{
					text += ":N";
				}
				if (createAlphaFromGrayscale)
				{
					text += ":A";
				}
				if (createNormalFromBump)
				{
					text = text + ":BN" + bumpStrength;
				}
				if (invert)
				{
					text += ":I";
				}
				return text;
			}
		}

		public void CreateTexture()
		{
			if (tex == null)
			{
				tex = new Texture2D(width, height, textureFormat, createMipMaps, linear);
				tex.name = cacheSignature;
			}
		}

		public void Process()
		{
			if (processed)
			{
				return;
			}
			if (imgPath != null && imgPath != "NULL")
			{
				if (FileManager.FileExists(imgPath))
				{
					try
					{
						using FileEntryStream fileEntryStream = FileManager.OpenStream(imgPath);
						Stream stream = fileEntryStream.Stream;
						Bitmap bitmap = new Bitmap(stream);
						SolidBrush solidBrush = new SolidBrush(System.Drawing.Color.White);
						bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
						if (!setSize)
						{
							width = bitmap.Width;
							height = bitmap.Height;
						}
						textureFormat = TextureFormat.BGRA32;
						Bitmap bitmap2 = new Bitmap(width, height, PixelFormat.Format32bppArgb);
						System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap2);
						Rectangle rect = new Rectangle(0, 0, width, height);
						if (setSize)
						{
							if (fillBackground)
							{
								graphics.FillRectangle(solidBrush, rect);
							}
							float num = Mathf.Min((float)width / (float)bitmap.Width, (float)height / (float)bitmap.Height);
							int num2 = (int)((float)bitmap.Width * num);
							int num3 = (int)((float)bitmap.Height * num);
							graphics.DrawImage(bitmap, (width - num2) / 2, (height - num3) / 2, num2, num3);
						}
						else
						{
							graphics.DrawImage(bitmap, 0, 0, width, height);
						}
						BitmapData bitmapData = bitmap2.LockBits(rect, ImageLockMode.ReadOnly, bitmap2.PixelFormat);
						int num4 = width * height;
						int num5 = num4 * 4;
						raw = new byte[num5 * 2];
						Marshal.Copy(bitmapData.Scan0, raw, 0, num5);
						bitmap2.UnlockBits(bitmapData);
						if (invert)
						{
							for (int i = 0; i < num5; i++)
							{
								int num6 = 255 - raw[i];
								raw[i] = (byte)num6;
							}
						}
						if (createAlphaFromGrayscale)
						{
							for (int j = 0; j < num5; j += 4)
							{
								int num7 = raw[j];
								int num8 = raw[j + 1];
								int num9 = raw[j + 2];
								int num10 = (num7 + num8 + num9) / 3;
								raw[j + 3] = (byte)num10;
							}
						}
						if (createNormalFromBump)
						{
							byte[] array = new byte[num5 * 2];
							float[][] array2 = new float[height][];
							for (int k = 0; k < height; k++)
							{
								array2[k] = new float[width];
								for (int l = 0; l < width; l++)
								{
									int num11 = (k * width + l) * 4;
									int num12 = raw[num11];
									int num13 = raw[num11 + 1];
									int num14 = raw[num11 + 2];
									float num15 = (float)(num12 + num13 + num14) / 768f;
									array2[k][l] = num15;
								}
							}
							Vector3 vector = default(Vector3);
							for (int m = 0; m < height; m++)
							{
								for (int n = 0; n < width; n++)
								{
									float num16 = 0.5f;
									float num17 = 0.5f;
									float num18 = 0.5f;
									float num19 = 0.5f;
									float num20 = 0.5f;
									float num21 = 0.5f;
									float num22 = 0.5f;
									float num23 = 0.5f;
									int num24 = n - 1;
									int num25 = n + 1;
									int num26 = m + 1;
									int num27 = m - 1;
									int num28 = num26;
									int num29 = num24;
									int num30 = num27;
									int num31 = num24;
									int num32 = num26;
									int num33 = num25;
									int num34 = num27;
									int num35 = num25;
									if (num28 >= 0 && num28 < height && num29 >= 0 && num29 < width)
									{
										num16 = array2[num28][num29];
									}
									if (num24 >= 0 && num24 < width)
									{
										num17 = array2[m][num24];
									}
									if (num30 >= 0 && num30 < height && num31 >= 0 && num31 < width)
									{
										num18 = array2[num30][num31];
									}
									if (num26 >= 0 && num26 < height)
									{
										num19 = array2[num26][n];
									}
									if (num27 >= 0 && num27 < height)
									{
										num20 = array2[num27][n];
									}
									if (num32 >= 0 && num32 < height && num33 >= 0 && num33 < width)
									{
										num21 = array2[num32][num33];
									}
									if (num25 >= 0 && num25 < width)
									{
										num22 = array2[m][num25];
									}
									if (num34 >= 0 && num34 < height && num35 >= 0 && num35 < width)
									{
										num23 = array2[num34][num35];
									}
									float num36 = num21 + 2f * num22 + num23 - num16 - 2f * num17 - num18;
									float num37 = num18 + 2f * num20 + num23 - num16 - 2f * num19 - num21;
									vector.x = num36 * bumpStrength;
									vector.y = num37 * bumpStrength;
									vector.z = 1f;
									vector.Normalize();
									vector.x = vector.x * 0.5f + 0.5f;
									vector.y = vector.y * 0.5f + 0.5f;
									vector.z = vector.z * 0.5f + 0.5f;
									int num38 = (int)(vector.x * 255f);
									int num39 = (int)(vector.y * 255f);
									int num40 = (int)(vector.z * 255f);
									int num41 = (m * width + n) * 4;
									array[num41] = (byte)num40;
									array[num41 + 1] = (byte)num39;
									array[num41 + 2] = (byte)num38;
									array[num41 + 3] = byte.MaxValue;
								}
							}
							raw = array;
						}
						solidBrush.Dispose();
						graphics.Dispose();
						bitmap.Dispose();
						bitmap2.Dispose();
					}
					catch (Exception ex)
					{
						hadError = true;
						errorText = ex.ToString();
					}
				}
				else
				{
					hadError = true;
					errorText = "Path " + imgPath + " is not valid";
				}
			}
			else
			{
				finished = true;
			}
			processed = true;
		}

		public void Finish()
		{
			if (hadError || finished)
			{
				return;
			}
			CreateTexture();
			if (tex.format != TextureFormat.BGRA32)
			{
				Texture2D texture2D = new Texture2D(width, height, TextureFormat.BGRA32, createMipMaps, linear);
				texture2D.LoadRawTextureData(raw);
				texture2D.Apply(updateMipmaps: true);
				texture2D.Compress(highQuality: true);
				byte[] rawTextureData = texture2D.GetRawTextureData();
				tex.LoadRawTextureData(rawTextureData);
				tex.Apply();
				UnityEngine.Object.Destroy(texture2D);
			}
			else
			{
				tex.LoadRawTextureData(raw);
				tex.Apply(updateMipmaps: true);
				if (compress)
				{
					tex.Compress(highQuality: true);
				}
			}
			finished = true;
		}

		public void DoCallback()
		{
			if (rawImageToLoad != null)
			{
				rawImageToLoad.texture = tex;
			}
			if (callback != null)
			{
				callback(this);
				callback = null;
			}
		}
	}

	protected class ImageLoaderTaskInfo
	{
		public string name;

		public AutoResetEvent resetEvent;

		public Thread thread;

		public volatile bool working;

		public volatile bool kill;
	}

	public static ImageLoaderThreaded singleton;

	public GameObject progressHUD;

	public Slider progressSlider;

	public Text progressText;

	protected ImageLoaderTaskInfo imageLoaderTask;

	protected bool _threadsRunning;

	protected Dictionary<string, Texture2D> thumbnailCache;

	protected Dictionary<string, Texture2D> textureCache;

	protected Dictionary<string, Texture2D> immediateTextureCache;

	protected Dictionary<Texture2D, bool> textureTrackedCache;

	protected Dictionary<Texture2D, int> textureUseCount;

	protected volatile Queue<QueuedImage> queuedImages;

	protected int numRealQueuedImages;

	protected int progress;

	protected int progressMax;

	protected AsyncFlag loadFlag;

	protected void MTTask(object info)
	{
		ImageLoaderTaskInfo imageLoaderTaskInfo = (ImageLoaderTaskInfo)info;
		while (_threadsRunning)
		{
			imageLoaderTaskInfo.resetEvent.WaitOne(-1, exitContext: true);
			if (imageLoaderTaskInfo.kill)
			{
				break;
			}
			ProcessImageQueueThreaded();
			imageLoaderTaskInfo.working = false;
		}
	}

	protected void StopThreads()
	{
		_threadsRunning = false;
		if (imageLoaderTask != null)
		{
			imageLoaderTask.kill = true;
			imageLoaderTask.resetEvent.Set();
			while (imageLoaderTask.thread.IsAlive)
			{
			}
			imageLoaderTask = null;
		}
	}

	protected void StartThreads()
	{
		if (!_threadsRunning)
		{
			_threadsRunning = true;
			imageLoaderTask = new ImageLoaderTaskInfo();
			imageLoaderTask.name = "ImageLoaderTask";
			imageLoaderTask.resetEvent = new AutoResetEvent(initialState: false);
			imageLoaderTask.thread = new Thread(MTTask);
			imageLoaderTask.thread.Priority = System.Threading.ThreadPriority.Normal;
			imageLoaderTask.thread.Start(imageLoaderTask);
		}
	}

	public bool RegisterTextureUse(Texture2D tex)
	{
		if (textureTrackedCache.ContainsKey(tex))
		{
			int value = 0;
			if (textureUseCount.TryGetValue(tex, out value))
			{
				textureUseCount.Remove(tex);
			}
			value++;
			textureUseCount.Add(tex, value);
			return true;
		}
		return false;
	}

	public bool DeregisterTextureUse(Texture2D tex)
	{
		int value = 0;
		if (textureUseCount.TryGetValue(tex, out value))
		{
			textureUseCount.Remove(tex);
			value--;
			if (value > 0)
			{
				textureUseCount.Add(tex, value);
			}
			else
			{
				textureUseCount.Remove(tex);
				textureCache.Remove(tex.name);
				textureTrackedCache.Remove(tex);
				UnityEngine.Object.Destroy(tex);
			}
			return true;
		}
		return false;
	}

	public void ReportOnTextures()
	{
		int num = 0;
		if (textureCache != null)
		{
			foreach (Texture2D value2 in textureCache.Values)
			{
				num++;
				int value = 0;
				if (textureUseCount.TryGetValue(value2, out value))
				{
					SuperController.LogMessage("Texture " + value2.name + " is in use " + value + " times");
				}
			}
		}
		SuperController.LogMessage("Using " + num + " textures");
	}

	public void PurgeAllTextures()
	{
		if (textureCache == null)
		{
			return;
		}
		foreach (Texture2D value in textureCache.Values)
		{
			UnityEngine.Object.Destroy(value);
		}
		textureUseCount.Clear();
		textureCache.Clear();
		textureTrackedCache.Clear();
	}

	public void PurgeAllImmediateTextures()
	{
		if (immediateTextureCache == null)
		{
			return;
		}
		foreach (Texture2D value in immediateTextureCache.Values)
		{
			UnityEngine.Object.Destroy(value);
		}
		immediateTextureCache.Clear();
	}

	public void ClearCacheThumbnail(string imgPath)
	{
		if (thumbnailCache != null && thumbnailCache.TryGetValue(imgPath, out var value))
		{
			thumbnailCache.Remove(imgPath);
			UnityEngine.Object.Destroy(value);
		}
	}

	public void ClearQueuedThumbnails()
	{
		if (imageLoaderTask == null)
		{
			return;
		}
		while (imageLoaderTask.working)
		{
			Thread.Sleep(0);
		}
		Queue<QueuedImage> queue = new Queue<QueuedImage>();
		foreach (QueuedImage queuedImage in queuedImages)
		{
			if (!queuedImage.isThumbnail)
			{
				queue.Enqueue(queuedImage);
			}
		}
		queuedImages = queue;
	}

	protected void ProcessImageQueueThreaded()
	{
		if (queuedImages != null && queuedImages.Count > 0)
		{
			QueuedImage queuedImage = queuedImages.Peek();
			queuedImage.Process();
		}
	}

	public Texture2D GetCachedThumbnail(string path)
	{
		if (thumbnailCache != null && thumbnailCache.TryGetValue(path, out var value))
		{
			return value;
		}
		return null;
	}

	public void QueueImage(QueuedImage qi)
	{
		if (queuedImages != null)
		{
			queuedImages.Enqueue(qi);
		}
		numRealQueuedImages++;
		progressMax++;
	}

	public void QueueThumbnail(QueuedImage qi)
	{
		qi.isThumbnail = true;
		if (queuedImages != null)
		{
			queuedImages.Enqueue(qi);
		}
	}

	public void ProcessImageImmediate(QueuedImage qi)
	{
		if (!qi.skipCache && immediateTextureCache != null && immediateTextureCache.TryGetValue(qi.cacheSignature, out var value))
		{
			UseCachedTex(qi, value);
		}
		qi.Process();
		qi.Finish();
		if (!qi.skipCache && !immediateTextureCache.ContainsKey(qi.cacheSignature) && qi.tex != null)
		{
			immediateTextureCache.Add(qi.cacheSignature, qi.tex);
		}
	}

	protected void PostProcessImageQueue()
	{
		if (queuedImages == null || queuedImages.Count <= 0)
		{
			return;
		}
		QueuedImage queuedImage = queuedImages.Peek();
		if (queuedImage.processed)
		{
			queuedImages.Dequeue();
			if (!queuedImage.isThumbnail)
			{
				progress++;
				numRealQueuedImages--;
				if (numRealQueuedImages == 0)
				{
					progress = 0;
					progressMax = 0;
					if (progressHUD != null)
					{
						progressHUD.SetActive(value: false);
					}
				}
				else
				{
					if (progressHUD != null)
					{
						progressHUD.SetActive(value: true);
					}
					if (progressSlider != null)
					{
						progressSlider.maxValue = progressMax;
						progressSlider.value = progress;
					}
				}
			}
			queuedImage.Finish();
			if (!queuedImage.skipCache && queuedImage.imgPath != null && queuedImage.imgPath != "NULL")
			{
				if (queuedImage.isThumbnail)
				{
					if (!thumbnailCache.ContainsKey(queuedImage.imgPath) && queuedImage.tex != null)
					{
						thumbnailCache.Add(queuedImage.imgPath, queuedImage.tex);
					}
				}
				else if (!textureCache.ContainsKey(queuedImage.cacheSignature) && queuedImage.tex != null)
				{
					textureCache.Add(queuedImage.cacheSignature, queuedImage.tex);
					textureTrackedCache.Add(queuedImage.tex, value: true);
				}
			}
			queuedImage.DoCallback();
		}
		if (numRealQueuedImages != 0)
		{
			if (loadFlag == null)
			{
				loadFlag = new AsyncFlag("ImageLoader");
				SuperController.singleton.SetLoadingIconFlag(loadFlag);
			}
		}
		else if (loadFlag != null)
		{
			loadFlag.Raise();
			loadFlag = null;
		}
	}

	protected void UseCachedTex(QueuedImage qi, Texture2D tex)
	{
		qi.tex = tex;
		if (qi.forceReload)
		{
			qi.width = tex.width;
			qi.height = tex.height;
			qi.setSize = true;
			qi.fillBackground = false;
		}
		else
		{
			qi.processed = true;
			qi.finished = true;
		}
	}

	protected void PreprocessImageQueue()
	{
		QueuedImage queuedImage = queuedImages.Peek();
		if (queuedImage == null)
		{
			return;
		}
		if (!queuedImage.skipCache && queuedImage.imgPath != null && queuedImage.imgPath != "NULL")
		{
			Texture2D value;
			if (queuedImage.isThumbnail)
			{
				if (thumbnailCache != null && thumbnailCache.TryGetValue(queuedImage.imgPath, out value))
				{
					if (value == null)
					{
						Debug.LogError("Trying to use cached texture at " + queuedImage.imgPath + " after it has been destroyed");
						thumbnailCache.Remove(queuedImage.imgPath);
					}
					else
					{
						UseCachedTex(queuedImage, value);
					}
				}
			}
			else if (textureCache != null && textureCache.TryGetValue(queuedImage.cacheSignature, out value))
			{
				if (value == null)
				{
					Debug.LogError("Trying to use cached texture at " + queuedImage.imgPath + " after it has been destroyed");
					textureCache.Remove(queuedImage.cacheSignature);
					textureTrackedCache.Remove(value);
				}
				else
				{
					UseCachedTex(queuedImage, value);
				}
			}
		}
		if (!queuedImage.isThumbnail && progressText != null)
		{
			progressText.text = "[" + progress + "/" + progressMax + "] " + queuedImage.imgPath;
		}
	}

	private void Update()
	{
		StartThreads();
		if (!imageLoaderTask.working)
		{
			PostProcessImageQueue();
			if (queuedImages != null && queuedImages.Count > 0)
			{
				PreprocessImageQueue();
				imageLoaderTask.working = true;
				imageLoaderTask.resetEvent.Set();
			}
		}
	}

	protected void OnDestroy()
	{
		if (Application.isPlaying)
		{
			StopThreads();
		}
		if (loadFlag != null)
		{
			loadFlag.Raise();
		}
		PurgeAllTextures();
		PurgeAllImmediateTextures();
	}

	protected void OnApplicationQuit()
	{
		if (Application.isPlaying)
		{
			StopThreads();
		}
	}

	private void Awake()
	{
		singleton = this;
		immediateTextureCache = new Dictionary<string, Texture2D>();
		textureCache = new Dictionary<string, Texture2D>();
		textureTrackedCache = new Dictionary<Texture2D, bool>();
		thumbnailCache = new Dictionary<string, Texture2D>();
		textureUseCount = new Dictionary<Texture2D, int>();
		queuedImages = new Queue<QueuedImage>();
	}
}
