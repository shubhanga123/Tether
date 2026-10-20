package app.tether

import android.app.Activity
import android.graphics.Color
import android.graphics.Typeface
import android.os.Bundle
import android.view.Gravity
import android.widget.LinearLayout
import android.widget.TextView

class MainActivity : Activity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val root = LinearLayout(this)
        root.orientation = LinearLayout.VERTICAL
        root.gravity = Gravity.CENTER
        root.setBackgroundColor(Color.parseColor("#1F8A4C"))
        root.setPadding(48, 48, 48, 48)

        val title = TextView(this)
        title.text = "Tether"
        title.textSize = 40f
        title.setTextColor(Color.WHITE)
        title.typeface = Typeface.DEFAULT_BOLD
        title.gravity = Gravity.CENTER

        val status = TextView(this)
        status.text = "Phone app is working\nStep 1 - version 0.1.0"
        status.textSize = 20f
        status.setTextColor(Color.WHITE)
        status.gravity = Gravity.CENTER
        status.setPadding(0, 32, 0, 0)

        root.addView(title)
        root.addView(status)
        setContentView(root)
    }
}
