using System;
using UnityEngine;

namespace VoiceChat
{
	[RequireComponent(typeof(AudioSource))]
	public class collision : MonoBehaviour
	{
		float lastTime = 0;
		double played = 0;
		double received = 0;
		int index = 0;
		float[] data;
		float playDelay = 0;
		bool shouldPlay = false;
		float lastRecvTime = 0;
		NSpeex.SpeexDecoder speexDec = new NSpeex.SpeexDecoder(NSpeex.BandMode.Narrow);
		//-----------
		public GameObject audioListener;
		public GameObject audioSource;
		float ListenerDistance;
		float VolumeFallOff = 1;
		Vector3 ListenerPosition;
		float sourceVolume = 10;
		float PanThreshold = 1;
		public bool ThreeD = true;
		
		[SerializeField]
		int playbackDelay = 2;
		
		public float LastRecvTime
		{
			get { return lastRecvTime; }
		}
		
		void Start()
		{
			int size = VoiceChatSettings.Instance.Frequency * 10;
			
			audioListener = GameObject.Find("FPSController");
			audioSource = GameObject.Find("VoiceChat_NetworkProxy(Clone)");
			
			GetComponent<AudioSource>().loop = true;
			GetComponent<AudioSource>().clip = AudioClip.Create("VoiceChat", size, 1, VoiceChatSettings.Instance.Frequency, false);
			data = new float[size];
			
			if (VoiceChatSettings.Instance.LocalDebug)
			{
				VoiceChatRecorder.Instance.NewSample += OnNewSample;
			}
		}
		
		public void OnTriggerStay(Collider collision)
		{
			if (GetComponent<AudioSource>().isPlaying)
			{
				// Wrapped around
				if (lastTime > GetComponent<AudioSource>().time)
				{
					played += GetComponent<AudioSource>().clip.length;
				}
				
				lastTime = GetComponent<AudioSource>().time;
				
				
				// Check if we've played to far
				if (played + GetComponent<AudioSource>().time >= received)
				{
					Stop();
					shouldPlay = false;
				}
			}
			else
			{
				if (shouldPlay)
				{
					playDelay -= Time.deltaTime;
					
					if (playDelay <= 0)
					{
						GetComponent<AudioSource>().Play();
					}
				}
			}

			if(collision.tag == "EyeSight"){
				GetComponent<AudioSource>().volume = (sourceVolume / 5);
				Debug.Log("cube1 hit OnCollisionEnter with " + collision.gameObject);
			}
			
			
		}
		
		void Stop()
		{
			GetComponent<AudioSource>().Stop();
			GetComponent<AudioSource>().time = 0;
			index = 0;
			played = 0;
			received = 0;
			lastTime = 0;
		}
		
		public void OnNewSample(VoiceChatPacket packet)
		{
			// Store last packet
			
			// Set last time we got something
			lastRecvTime = Time.time;
			
			// Decompress
			float[] sample = null;
			int length = VoiceChatUtils.Decompress(speexDec, packet, out sample);
			
			// Add more time to received
			received += VoiceChatSettings.Instance.SampleTime;
			
			// Push data to buffer
			Array.Copy(sample, 0, data, index, length);
			
			// Increase index
			index += length;
			
			// Handle wrap-around
			if (index >= GetComponent<AudioSource>().clip.samples)
			{
				index = 0;
			}
			
			// Set data
			GetComponent<AudioSource>().clip.SetData(data, 0);
			
			// If we're not playing
			if (!GetComponent<AudioSource>().isPlaying)
			{
				// Set that we should be playing
				shouldPlay = true;
				
				// And if we have no delay set, set it.
				if (playDelay <= 0)
				{
					playDelay = (float)VoiceChatSettings.Instance.SampleTime * playbackDelay;
				}
			}
			
			VoiceChatFloatPool.Instance.Return(sample);
		}
	} 
}