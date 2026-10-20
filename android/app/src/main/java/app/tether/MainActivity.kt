package app.tether

import android.app.Activity
import android.os.Bundle
import android.widget.Button
import android.widget.EditText
import android.widget.TextView
import java.io.BufferedReader
import java.io.InputStreamReader
import java.io.PrintWriter
import java.net.Socket
import java.security.cert.X509Certificate
import javax.net.ssl.SSLContext
import javax.net.ssl.TrustManager
import javax.net.ssl.X509TrustManager

class MainActivity : Activity() {

    private lateinit var ipInput: EditText
    private lateinit var codeInput: EditText
    private lateinit var connectButton: Button
    private lateinit var statusText: TextView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        ipInput = findViewById(R.id.ipInput)
        codeInput = findViewById(R.id.codeInput)
        connectButton = findViewById(R.id.connectButton)
        statusText = findViewById(R.id.statusText)

        connectButton.setOnClickListener {
            val ip = ipInput.text.toString().trim()
            val code = codeInput.text.toString().trim()

            if (ip.isEmpty() || code.isEmpty()) {
                statusText.text = "Please enter both IP and Code."
                return@setOnClickListener
            }

            statusText.text = "Connecting to $ip..."
            connectToPc(ip, code)
        }
    }

    private fun connectToPc(ip: String, code: String) {
        Thread {
            try {
                val trustAllCerts = arrayOf<TrustManager>(object : X509TrustManager {
                    override fun checkClientTrusted(chain: Array<out X509Certificate>?, authType: String?) {}
                    override fun checkServerTrusted(chain: Array<out X509Certificate>?, authType: String?) {}
                    override fun getAcceptedIssuers(): Array<X509Certificate> = arrayOf()
                })

                val sslContext = SSLContext.getInstance("TLS")
                sslContext.init(null, trustAllCerts, java.security.SecureRandom())

                val socket: Socket = sslContext.socketFactory.createSocket(ip, 8443)
                val writer = PrintWriter(socket.getOutputStream(), true)
                val reader = BufferedReader(InputStreamReader(socket.getInputStream()))

                writer.println("PAIR:$code")

                val response = reader.readLine()

                runOnUiThread {
                    if (response == "OK") {
                        statusText.text = "Connected!"
                        statusText.setTextColor(0xFF2ECC71.toInt())
                    } else {
                        statusText.text = "Pairing failed: $response"
                    }
                }

                socket.close()
            } catch (e: Exception) {
                runOnUiThread {
                    statusText.text = "Error: ${e.localizedMessage}"
                }
            }
        }.start()
    }
}
