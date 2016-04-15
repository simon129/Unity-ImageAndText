using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class TextAndImage : MonoBehaviour
{
	/// <summary>
	/// 我們要找出所有的format參數 ex. {0}, {1:N}, {2:YYYY-MM-DD}
	/// 最重要的是我們要找出{0:image}這種format把它變換成相對應的圖片
	/// </summary>
	const string PATTERN = @"{(\d+)(:([^}]+))?}";
	
	/// <summary>const for {0:image}</summary>
	const string IMAGE_FORMAT = "image";

	/// <summary>我們用來複製用的Text, 大小顏色鮮都先準備好</summary>
	public Text SampleText;

	/// <summary>text pool</summary>
	List<Text> mTextPool = new List<Text>();
	/// <summary>text pool index</summary>
	int mTextIndex = 0;

	/// <summary>image pool</summary>
	List<Image> mImgPool = new List<Image>();
	/// <summary>image pool index</summary>
	int mImgIndex = 0;


	/// <summary>目前處理到的位置</summary>
	int mIndex = 0;
	/// <summary>下一個物件的sibling index</summary>
	int mSiblingIndex = 0;

	Regex mRegex;

	#region Demo用

	/// <summary>我們要使用的Sprite資料, 這邊沒有處理從外部load Sprite的部分</summary>
	public Sprite[] SpriteData;

	/// <summary>我們要使用的Sprite資料, 這邊沒有處理從外部load Sprite的部分</summary>
	/// <example>
	/// {0} 使用 {1} {2:image} 擊殺了你
	/// Destroyed by {0} ({1}) {2:image}
	/// Убил: {0}, оружие: {1} {2:image}
	/// </example>
	public string format = "{0} 使用 {1} {2:image} 擊殺了你";

	void Start()
	{
		Format(format, "simon", "火箭筒", SpriteData[0]);
	}

	#endregion Demo用

	void Awake()
	{
		mRegex = new Regex(PATTERN, RegexOptions.IgnoreCase);
	}

	public void Format(string format, params object[] args)
	{
		// reset all indexes
		mIndex = mSiblingIndex = mTextIndex = mImgIndex = 0;

		// 把上次的都關掉
		mTextPool.ForEach(t => t.gameObject.SetActive(false));
		mImgPool.ForEach(i => i.gameObject.SetActive(false));

		MatchCollection matches = mRegex.Matches(format);
		for (int i = 0; i < matches.Count; i++)
		{
			Match match = matches[i];
			GroupCollection groups = match.Groups;
			/*
			 * {0}
			 * [0] => {0}
			 * [1] => 0
			 * [2] =>
			 * [3] =>
			 * 
			 * {1:image}
			 * [0] => {1:image}
			 * [1] => 1
			 * [2] => :image
			 * [3] => image
			 */ 
			if (match.Groups.Count > 3 && match.Groups[3].Value == IMAGE_FORMAT) // image
			{
				// 把index之前的字切出來
				// (string format, object[] args, int index, int length)
				GetText(format, args, mIndex, match.Index - mIndex);
				// 把image 切出來
				// (string format, object[] args, int index, int length, string argsValue)
				GetImg(format, args, mIndex, match.Groups[0].Length, match.Groups[1].Value);
			}
		}

		// 處理最後面剩下的文字
		if (mIndex < format.Length)
		{
			// (string format, object[] args, int index, int length)
			GetText(format, args, mIndex, format.Length - mIndex);
		}
	}

	private void GetText(string format, object[] args, int index, int length)
	{
		Text text;
		if (mTextIndex >= mTextPool.Count)
		{
			text = GameObject.Instantiate(SampleText) as Text;
			text.transform.SetParent(transform, false);
			mTextPool.Add(text);
		}
		else
		{
			text = mTextPool[mTextIndex];
		}

		var sub = format.Substring(index, length);
		text.text = string.Format(sub, args);
		text.transform.SetSiblingIndex(mSiblingIndex);
		text.gameObject.SetActive(true);

		mTextIndex++;
		mSiblingIndex++;
		mIndex = index + length;
	}

	private void GetImg(string format, object[] args, int index, int length, string argsValue)
	{
		// 把{3:img} 這個 3 抓出來
		int argsIndex;
		if (System.Int32.TryParse(argsValue, out argsIndex))
		{
			if (argsIndex < 0 || argsIndex >= args.Length)
				throw new System.IndexOutOfRangeException(string.Format("args.Length={0}, image index={1}", args.Length, argsIndex));

			Image img;
			if (mImgIndex >= mImgPool.Count)
			{
				img = new GameObject("Image").AddComponent<Image>();
				img.transform.SetParent(transform, false);
				img.transform.localPosition = Vector3.zero;
				img.transform.localScale = Vector3.one;
				img.transform.localRotation = Quaternion.identity;
				img.gameObject.layer = gameObject.layer;
				mImgPool.Add(img);
			}
			else
			{
				img = mImgPool[mImgIndex];
			}

			img.sprite = args[argsIndex] as Sprite;
			img.SetNativeSize();
			img.transform.SetSiblingIndex(mSiblingIndex);

			img.gameObject.SetActive(true);
		}
		else
		{
			Debug.LogError("img index parse error");
		}

		mImgIndex++;
		mSiblingIndex++;
		mIndex = index + length;
	}
}