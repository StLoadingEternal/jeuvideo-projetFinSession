using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace SlimUI.ModernMenu{
	public class UISettingsManager : MonoBehaviour {

		// toggle buttons
	
		[Header("GAME SETTINGS")]
		public GameObject showhudtext;
		public GameObject tooltipstext;


		// sliders
		public GameObject musicSlider;
		

		
		public void Start()
		{
			// check slider values
			musicSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MusicVolume");
		}

		public void MusicSlider (){
			//PlayerPrefs.SetFloat("MusicVolume", sliderValue);
			PlayerPrefs.SetFloat("MusicVolume", musicSlider.GetComponent<Slider>().value);
		}
	}
}