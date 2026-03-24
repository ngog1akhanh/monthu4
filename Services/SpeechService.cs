using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Media;
using System.Reflection;

namespace TourGuideSmart.Services
{
    public class SpeechService
    {
        // Keep a single synthesizer active so multiple clicks don't spawn overlapping speech.
        private static dynamic? _currentSynth;
        private static readonly object _sync = new object();

        public void Speak(string text, string? preferredGoogleVoice = null)
        {
            // Try ElevenLabs TTS if API key provided via environment variable ELEVENLABS_API_KEY.
            var elevenKey = Environment.GetEnvironmentVariable("ELEVENLABS_API_KEY");
            var elevenVoice = preferredGoogleVoice ?? "21m00Tcm4TlvDq8ikWAM"; // default sample voice id
            if (!string.IsNullOrEmpty(elevenKey))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        using var client = new HttpClient();
                        client.DefaultRequestHeaders.Add("xi-api-key", elevenKey);

                        string safeText = text.Replace("\\", "\\\\").Replace("\"", "\\\"");
                        var json = $"{{\"text\":\"{safeText}\",\"model_id\":\"eleven_multilingual_v2\"}}";
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var resp = await client.PostAsync($"https://api.elevenlabs.io/v1/text-to-speech/{elevenVoice}", content);
                        if (resp.IsSuccessStatusCode)
                        {
                            var bytes = await resp.Content.ReadAsByteArrayAsync();
                            var tmp = Path.Combine(Path.GetTempPath(), "tts_" + Guid.NewGuid().ToString() + ".mp3");
                            await File.WriteAllBytesAsync(tmp, bytes);
                            try { Process.Start(new ProcessStartInfo { FileName = tmp, UseShellExecute = true }); } catch { }
                            try { File.Delete(tmp); } catch { }
                            return;
                        }



                    }
                    catch { }

                    // if ElevenLabs failed, fall back to local methods
                    PlayLocalFallback(text);
                });

                return;
            }

            // no ElevenLabs key: use local fallbacks
            PlayLocalFallback(text);
        }

        private void PlayLocalFallback(string text)
        {
            // Try COM SAPI (available on most Windows machines) next so user hears immediate speech.
            try
            {
                var sapiType = Type.GetTypeFromProgID("SAPI.SpVoice");
                if (sapiType != null)
                {
                    dynamic sapi = Activator.CreateInstance(sapiType)!;
                    try
                    {
                        // synchronous speak is fine for short texts
                        sapi.Speak(text);
                    }
                    finally
                    {
                        try { Marshal.ReleaseComObject(sapi); } catch { }
                    }

                    return;
                }
            }
            catch { }

            // Try to use System.Speech.Synthesis.SpeechSynthesizer if available and play asynchronously so UI isn't blocked.
            try
            {
                Type? synthType = Type.GetType("System.Speech.Synthesis.SpeechSynthesizer, System.Speech");
                if (synthType != null)
                {
                    lock (_sync)
                    {
                        try
                        {
                            if (_currentSynth != null)
                            {
                                try { _currentSynth.SpeakAsyncCancelAll(); } catch { }
                                try { _currentSynth.Dispose(); } catch { }
                                _currentSynth = null;
                            }
                        }
                        catch { }

                        _currentSynth = Activator.CreateInstance(synthType)!;

                        try
                        {
                            // start async so it plays immediately without blocking UI
                            try { _currentSynth.SpeakAsync(text); } catch
                            {
                                // fallback to sync speak if async not supported for some reason
                                _currentSynth.Speak(text);
                                try { _currentSynth.Dispose(); } catch { }
                                _currentSynth = null;
                            }

                            return;
                        }
                        catch
                        {
                            try { _currentSynth.Dispose(); } catch { }
                            _currentSynth = null;
                        }
                    }
                }
            }
            catch
            {
                // ignore and fallback
            }

            // Fallback: show text so user still gets the content.
            try { MessageBox.Show(text, "Speech (fallback)", MessageBoxButtons.OK, MessageBoxIcon.Information); } catch { }
        }

        private static byte[] ConvertPcmToWav(byte[] pcm, int channels, int sampleRate, int bitsPerSample)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            int byteRate = sampleRate * channels * bitsPerSample / 8;
            int blockAlign = channels * bitsPerSample / 8;

            // RIFF header
            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + pcm.Length); // file size - 8
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            // fmt chunk
            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16); // subchunk1 size
            bw.Write((short)1); // PCM
            bw.Write((short)channels);
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write((short)blockAlign);
            bw.Write((short)bitsPerSample);

            // data chunk
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write(pcm.Length);
            bw.Write(pcm);

            bw.Flush();
            return ms.ToArray();
        }
    }
}
