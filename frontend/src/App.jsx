import { useState, useEffect, useRef } from "react";

const theme = `
  @import url('https://fonts.googleapis.com/css2?family=Playfair+Display:wght@400;700;900&family=DM+Sans:wght@300;400;500&display=swap');

  :root {
    --ink: #0e0c0a;
    --cream: #faf7f2;
    --gold: #c9a84c;
    --gold-light: #e8d5a3;
    --gold-dark: #8a6f2e;
    --smoke: #f0ece4;
    --muted: #9c9488;
    --blade: #2a2520;
    --red: #c0392b;
    --green: #2e7d52;
    --white: #ffffff;
  }

  * { box-sizing: border-box; margin: 0; padding: 0; }

  body {
    font-family: 'DM Sans', sans-serif;
    background: var(--cream);
    color: var(--ink);
    min-height: 100vh;
  }

  .playfair { font-family: 'Playfair Display', serif; }

  @keyframes fadeUp {
    from { opacity: 0; transform: translateY(20px); }
    to   { opacity: 1; transform: translateY(0); }
  }
  @keyframes pulse-gold {
    0%,100% { box-shadow: 0 0 0 0 rgba(201,168,76,0.4); }
    50%      { box-shadow: 0 0 0 8px rgba(201,168,76,0); }
  }
  @keyframes spin {
    to { transform: rotate(360deg); }
  }
  @keyframes slideIn {
    from { transform: translateX(100%); opacity: 0; }
    to   { transform: translateX(0);   opacity: 1; }
  }
  .fade-up { animation: fadeUp 0.5s ease forwards; }
`;

// ─── Mock Data ────────────────────────────────────────────────────
const MOCK_QUEUE = [
  { id: 1, tokenNumber: 47, customerName: "Rahul Sharma",   phone: "98765xxxxx", status: "Serving",  joinedAt: "10:32 AM", wait: 0  },
  { id: 2, tokenNumber: 48, customerName: "Amit Verma",     phone: "87654xxxxx", status: "Waiting",  joinedAt: "10:45 AM", wait: 10 },
  { id: 3, tokenNumber: 49, customerName: "Priya Singh",    phone: "76543xxxxx", status: "Waiting",  joinedAt: "10:51 AM", wait: 20 },
  { id: 4, tokenNumber: 50, customerName: "Deepak Kumar",   phone: "65432xxxxx", status: "Waiting",  joinedAt: "11:02 AM", wait: 30 },
  { id: 5, tokenNumber: 51, customerName: "Suresh Yadav",   phone: "54321xxxxx", status: "Waiting",  joinedAt: "11:10 AM", wait: 40 },
];

// ─── Tiny Components ──────────────────────────────────────────────
const Badge = ({ children, color = "gold" }) => {
  const colors = {
    gold:  { bg: "#fdf3d8", text: "#8a6f2e", border: "#e8d5a3" },
    green: { bg: "#e8f5ee", text: "#2e7d52", border: "#a8d5bb" },
    red:   { bg: "#fdecea", text: "#c0392b", border: "#f5bcb6" },
    gray:  { bg: "#f0ece4", text: "#9c9488", border: "#ddd8ce" },
  };
  const c = colors[color];
  return (
    <span style={{
      background: c.bg, color: c.text, border: `1px solid ${c.border}`,
      padding: "2px 10px", borderRadius: 20, fontSize: 11,
      fontWeight: 500, letterSpacing: "0.04em", whiteSpace: "nowrap"
    }}>{children}</span>
  );
};

const GoldDivider = () => (
  <div style={{ display: "flex", alignItems: "center", gap: 12, margin: "20px 0" }}>
    <div style={{ flex: 1, height: 1, background: "linear-gradient(90deg, transparent, var(--gold-light))" }} />
    <div style={{ width: 6, height: 6, background: "var(--gold)", transform: "rotate(45deg)" }} />
    <div style={{ flex: 1, height: 1, background: "linear-gradient(90deg, var(--gold-light), transparent)" }} />
  </div>
);

// ─── SCREEN 1: Landing / Home ─────────────────────────────────────
function LandingScreen({ onNavigate }) {
  return (
    <div style={{ minHeight: "100vh", background: "var(--ink)", color: "var(--cream)", position: "relative", overflow: "hidden" }}>
      {/* Decorative background */}
      <div style={{
        position: "absolute", inset: 0,
        backgroundImage: `radial-gradient(ellipse 80% 60% at 50% -10%, rgba(201,168,76,0.15) 0%, transparent 70%)`,
        pointerEvents: "none"
      }} />
      <div style={{
        position: "absolute", top: 0, right: 0, width: 400, height: 400,
        borderRadius: "50%", background: "rgba(201,168,76,0.04)",
        transform: "translate(30%, -30%)"
      }} />

      {/* Nav */}
      <nav style={{
        display: "flex", justifyContent: "space-between", alignItems: "center",
        padding: "24px 40px", borderBottom: "1px solid rgba(201,168,76,0.15)",
        position: "relative", zIndex: 10
      }}>
        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <div style={{
            width: 36, height: 36, background: "var(--gold)",
            borderRadius: "50%", display: "flex", alignItems: "center", justifyContent: "center",
            fontSize: 18
          }}>✂</div>
          <span style={{ fontFamily: "Playfair Display, serif", fontSize: 22, fontWeight: 700, letterSpacing: "0.02em" }}>
            CutBook
          </span>
        </div>
        <div style={{ display: "flex", gap: 12 }}>
          <button onClick={() => onNavigate("login")} style={{
            background: "transparent", border: "1px solid rgba(201,168,76,0.4)",
            color: "var(--gold)", padding: "10px 24px", borderRadius: 6, cursor: "pointer",
            fontFamily: "DM Sans, sans-serif", fontSize: 14, fontWeight: 500,
            transition: "all 0.2s"
          }}>Login</button>
          <button onClick={() => onNavigate("register")} style={{
            background: "var(--gold)", border: "none",
            color: "var(--ink)", padding: "10px 24px", borderRadius: 6, cursor: "pointer",
            fontFamily: "DM Sans, sans-serif", fontSize: 14, fontWeight: 600,
            transition: "all 0.2s"
          }}>Free Shuru Karo</button>
        </div>
      </nav>

      {/* Hero */}
      <div style={{ maxWidth: 900, margin: "0 auto", padding: "100px 40px 80px", textAlign: "center", position: "relative", zIndex: 5 }}>
        <div style={{ display: "inline-flex", alignItems: "center", gap: 8, background: "rgba(201,168,76,0.1)", border: "1px solid rgba(201,168,76,0.3)", borderRadius: 20, padding: "6px 16px", marginBottom: 32 }}>
          <span style={{ width: 6, height: 6, background: "var(--gold)", borderRadius: "50%", animation: "pulse-gold 2s infinite" }} />
          <span style={{ color: "var(--gold)", fontSize: 12, letterSpacing: "0.1em", fontWeight: 500 }}>INDIA'S SMART BARBER QUEUE</span>
        </div>

        <h1 style={{
          fontFamily: "Playfair Display, serif", fontSize: "clamp(42px, 7vw, 80px)",
          fontWeight: 900, lineHeight: 1.05, marginBottom: 28, color: "var(--cream)"
        }}>
          Barber shop ka<br />
          <span style={{ color: "var(--gold)" }}>wait khatam.</span><br />
          Paisa shuru.
        </h1>

        <p style={{ fontSize: 18, color: "#b8b0a4", lineHeight: 1.7, maxWidth: 560, margin: "0 auto 48px", fontWeight: 300 }}>
          Customer ghar se token lo. Barber ek button se next bulao.
          Real-time queue — koi bhi wait nahi karta.
        </p>

        <div style={{ display: "flex", gap: 16, justifyContent: "center", flexWrap: "wrap" }}>
          <button onClick={() => onNavigate("register")} style={{
            background: "var(--gold)", border: "none", color: "var(--ink)",
            padding: "16px 40px", borderRadius: 8, cursor: "pointer",
            fontFamily: "DM Sans, sans-serif", fontSize: 16, fontWeight: 700,
            letterSpacing: "0.02em", transition: "all 0.2s",
            boxShadow: "0 8px 32px rgba(201,168,76,0.3)"
          }}>Abhi Shuru Karo — Free</button>
          <button onClick={() => onNavigate("customer")} style={{
            background: "transparent", border: "1px solid rgba(250,247,242,0.2)",
            color: "var(--cream)", padding: "16px 40px", borderRadius: 8, cursor: "pointer",
            fontFamily: "DM Sans, sans-serif", fontSize: 16, fontWeight: 400,
          }}>Live Demo Dekho →</button>
        </div>

        <GoldDivider />

        {/* Stats */}
        <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 32, maxWidth: 560, margin: "0 auto" }}>
          {[["500+", "Barber shops"], ["50k+", "Tokens daily"], ["4.9★", "Rating"]].map(([val, label]) => (
            <div key={label}>
              <div style={{ fontFamily: "Playfair Display, serif", fontSize: 36, fontWeight: 700, color: "var(--gold)" }}>{val}</div>
              <div style={{ fontSize: 13, color: "#7a7268", marginTop: 4 }}>{label}</div>
            </div>
          ))}
        </div>
      </div>

      {/* Pricing */}
      <div style={{ background: "rgba(255,255,255,0.03)", borderTop: "1px solid rgba(201,168,76,0.1)", padding: "80px 40px" }}>
        <div style={{ maxWidth: 900, margin: "0 auto" }}>
          <h2 style={{ fontFamily: "Playfair Display, serif", fontSize: 40, fontWeight: 700, textAlign: "center", marginBottom: 8 }}>Simple Plans</h2>
          <p style={{ textAlign: "center", color: "#7a7268", marginBottom: 56, fontSize: 16 }}>Shuru free mein, badhao jab zaroorat ho</p>
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(240px, 1fr))", gap: 24 }}>
            {[
              { name: "Free",    price: "₹0",   per: "hamesha", features: ["20 tokens/month", "Live queue", "Basic dashboard"], cta: "Shuru Karo", highlight: false },
              { name: "Pro",     price: "₹299", per: "month",   features: ["Unlimited tokens", "WhatsApp alerts", "Priority support"], cta: "Pro Lo", highlight: true },
              { name: "Premium", price: "₹599", per: "month",   features: ["Sab kuch Pro mein", "Multiple staff", "Analytics"], cta: "Premium Lo", highlight: false },
            ].map(plan => (
              <div key={plan.name} style={{
                background: plan.highlight ? "var(--gold)" : "rgba(255,255,255,0.04)",
                border: `1px solid ${plan.highlight ? "var(--gold)" : "rgba(201,168,76,0.15)"}`,
                borderRadius: 16, padding: 32,
                transform: plan.highlight ? "scale(1.04)" : "none",
                transition: "all 0.2s"
              }}>
                <div style={{ fontSize: 12, letterSpacing: "0.12em", color: plan.highlight ? "var(--blade)" : "var(--muted)", marginBottom: 8, fontWeight: 600 }}>{plan.name.toUpperCase()}</div>
                <div style={{ fontFamily: "Playfair Display, serif", fontSize: 44, fontWeight: 900, color: plan.highlight ? "var(--ink)" : "var(--cream)", lineHeight: 1 }}>{plan.price}</div>
                <div style={{ fontSize: 13, color: plan.highlight ? "#5a4820" : "#7a7268", marginBottom: 28 }}>/{plan.per}</div>
                {plan.features.map(f => (
                  <div key={f} style={{ display: "flex", gap: 10, marginBottom: 12, alignItems: "center" }}>
                    <span style={{ color: plan.highlight ? "var(--ink)" : "var(--gold)", fontSize: 14 }}>✓</span>
                    <span style={{ fontSize: 14, color: plan.highlight ? "var(--ink)" : "#b8b0a4" }}>{f}</span>
                  </div>
                ))}
                <button onClick={() => onNavigate("register")} style={{
                  width: "100%", marginTop: 24, padding: "14px",
                  background: plan.highlight ? "var(--ink)" : "transparent",
                  border: `1px solid ${plan.highlight ? "var(--ink)" : "rgba(201,168,76,0.3)"}`,
                  color: plan.highlight ? "var(--gold)" : "var(--gold)",
                  borderRadius: 8, cursor: "pointer", fontFamily: "DM Sans, sans-serif",
                  fontSize: 14, fontWeight: 600, letterSpacing: "0.04em"
                }}>{plan.cta}</button>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

// ─── SCREEN 2: Auth (Login / Register) ───────────────────────────
function AuthScreen({ mode, onNavigate }) {
  const [form, setForm] = useState({ name: "", email: "", phone: "", password: "" });
  const [loading, setLoading] = useState(false);
  const isLogin = mode === "login";

  const handleSubmit = () => {
    setLoading(true);
    setTimeout(() => { setLoading(false); onNavigate("dashboard"); }, 1200);
  };

  return (
    <div style={{ minHeight: "100vh", display: "flex" }}>
      {/* Left panel */}
      <div style={{
        flex: "0 0 45%", background: "var(--ink)", display: "flex", flexDirection: "column",
        justifyContent: "center", padding: "60px 56px", position: "relative", overflow: "hidden"
      }}>
        <div style={{ position: "absolute", bottom: -80, right: -80, width: 300, height: 300, borderRadius: "50%", background: "rgba(201,168,76,0.06)" }} />
        <div style={{ position: "absolute", top: -40, left: -40, width: 200, height: 200, borderRadius: "50%", background: "rgba(201,168,76,0.04)" }} />
        <div style={{ cursor: "pointer" }} onClick={() => onNavigate("home")}>
          <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 64 }}>
            <div style={{ width: 36, height: 36, background: "var(--gold)", borderRadius: "50%", display: "flex", alignItems: "center", justifyContent: "center", fontSize: 18 }}>✂</div>
            <span style={{ fontFamily: "Playfair Display, serif", fontSize: 22, fontWeight: 700, color: "var(--cream)" }}>CutBook</span>
          </div>
        </div>
        <h2 style={{ fontFamily: "Playfair Display, serif", fontSize: 40, fontWeight: 900, color: "var(--cream)", lineHeight: 1.2, marginBottom: 20 }}>
          {isLogin ? "Wapas aao,\nUstaa." : "Apni shop\ndigital karo."}
        </h2>
        <p style={{ color: "#7a7268", fontSize: 15, lineHeight: 1.7, fontWeight: 300 }}>
          {isLogin ? "Queue manage karo, customers ko wait mat karo." : "5 minute mein setup. Free mein shuru. Kab bhi upgrade."}
        </p>
        <GoldDivider />
        <div style={{ display: "flex", gap: 24 }}>
          {["✂ Smart Queue", "📱 Real-time", "💰 Subscription"].map(f => (
            <div key={f} style={{ fontSize: 12, color: "#5a5248" }}>{f}</div>
          ))}
        </div>
      </div>

      {/* Right panel */}
      <div style={{ flex: 1, display: "flex", alignItems: "center", justifyContent: "center", padding: 40, background: "var(--cream)" }}>
        <div style={{ width: "100%", maxWidth: 420, animation: "fadeUp 0.4s ease" }}>
          <h1 style={{ fontFamily: "Playfair Display, serif", fontSize: 32, fontWeight: 700, marginBottom: 6 }}>
            {isLogin ? "Login" : "Account Banao"}
          </h1>
          <p style={{ color: "var(--muted)", fontSize: 14, marginBottom: 36 }}>
            {isLogin ? "Apna account mein wapas aao" : "Bilkul free — credit card nahi chahiye"}
          </p>

          <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
            {!isLogin && (
              <div>
                <label style={{ fontSize: 12, fontWeight: 500, letterSpacing: "0.06em", color: "var(--muted)", display: "block", marginBottom: 6 }}>POORA NAAM</label>
                <input value={form.name} onChange={e => setForm({...form, name: e.target.value})}
                  placeholder="Arjun Sharma"
                  style={{ width: "100%", padding: "14px 16px", border: "1.5px solid #e0dbd2", borderRadius: 8, fontSize: 15, fontFamily: "DM Sans, sans-serif", background: "var(--white)", outline: "none", transition: "border 0.2s" }}
                  onFocus={e => e.target.style.borderColor = "var(--gold)"}
                  onBlur={e => e.target.style.borderColor = "#e0dbd2"} />
              </div>
            )}
            <div>
              <label style={{ fontSize: 12, fontWeight: 500, letterSpacing: "0.06em", color: "var(--muted)", display: "block", marginBottom: 6 }}>EMAIL</label>
              <input value={form.email} onChange={e => setForm({...form, email: e.target.value})}
                placeholder="arjun@gmail.com" type="email"
                style={{ width: "100%", padding: "14px 16px", border: "1.5px solid #e0dbd2", borderRadius: 8, fontSize: 15, fontFamily: "DM Sans, sans-serif", background: "var(--white)", outline: "none", transition: "border 0.2s" }}
                onFocus={e => e.target.style.borderColor = "var(--gold)"}
                onBlur={e => e.target.style.borderColor = "#e0dbd2"} />
            </div>
            {!isLogin && (
              <div>
                <label style={{ fontSize: 12, fontWeight: 500, letterSpacing: "0.06em", color: "var(--muted)", display: "block", marginBottom: 6 }}>PHONE</label>
                <input value={form.phone} onChange={e => setForm({...form, phone: e.target.value})}
                  placeholder="+91 98765 43210"
                  style={{ width: "100%", padding: "14px 16px", border: "1.5px solid #e0dbd2", borderRadius: 8, fontSize: 15, fontFamily: "DM Sans, sans-serif", background: "var(--white)", outline: "none", transition: "border 0.2s" }}
                  onFocus={e => e.target.style.borderColor = "var(--gold)"}
                  onBlur={e => e.target.style.borderColor = "#e0dbd2"} />
              </div>
            )}
            <div>
              <label style={{ fontSize: 12, fontWeight: 500, letterSpacing: "0.06em", color: "var(--muted)", display: "block", marginBottom: 6 }}>PASSWORD</label>
              <input value={form.password} onChange={e => setForm({...form, password: e.target.value})}
                placeholder="••••••••" type="password"
                style={{ width: "100%", padding: "14px 16px", border: "1.5px solid #e0dbd2", borderRadius: 8, fontSize: 15, fontFamily: "DM Sans, sans-serif", background: "var(--white)", outline: "none", transition: "border 0.2s" }}
                onFocus={e => e.target.style.borderColor = "var(--gold)"}
                onBlur={e => e.target.style.borderColor = "#e0dbd2"} />
            </div>
          </div>

          <button onClick={handleSubmit} style={{
            width: "100%", marginTop: 28, padding: "16px",
            background: loading ? "#c4b98a" : "var(--gold)",
            border: "none", borderRadius: 8, cursor: loading ? "not-allowed" : "pointer",
            fontFamily: "DM Sans, sans-serif", fontSize: 15, fontWeight: 700,
            color: "var(--ink)", letterSpacing: "0.04em", transition: "all 0.2s",
            display: "flex", alignItems: "center", justifyContent: "center", gap: 8
          }}>
            {loading && <span style={{ width: 16, height: 16, border: "2px solid var(--ink)", borderTopColor: "transparent", borderRadius: "50%", animation: "spin 0.6s linear infinite" }} />}
            {loading ? "Checking..." : isLogin ? "Login Karo" : "Account Banao"}
          </button>

          <p style={{ textAlign: "center", marginTop: 20, fontSize: 14, color: "var(--muted)" }}>
            {isLogin ? "Account nahi hai? " : "Account hai? "}
            <span onClick={() => onNavigate(isLogin ? "register" : "login")}
              style={{ color: "var(--gold-dark)", fontWeight: 600, cursor: "pointer", textDecoration: "underline" }}>
              {isLogin ? "Register Karo" : "Login Karo"}
            </span>
          </p>
        </div>
      </div>
    </div>
  );
}

// ─── SCREEN 3: Barber Dashboard ───────────────────────────────────
function DashboardScreen({ onNavigate }) {
  const [queue, setQueue] = useState(MOCK_QUEUE);
  const [activeTab, setActiveTab] = useState("queue");
  const [toast, setToast] = useState(null);

  const showToast = (msg, type = "success") => {
    setToast({ msg, type });
    setTimeout(() => setToast(null), 3000);
  };

  const callNext = () => {
    setQueue(prev => {
      const waiting = prev.filter(e => e.status === "Waiting");
      if (!waiting.length) { showToast("Queue khali hai!", "info"); return prev; }
      return prev.map(e =>
        e.id === waiting[0].id ? { ...e, status: "Called" } :
        e.status === "Serving"  ? { ...e, status: "Done" }  : e
      );
    });
    showToast("Next customer bulaya!");
  };

  const markDone = (id) => {
    setQueue(prev => prev.map(e => e.id === id ? { ...e, status: "Done" } : e));
    showToast("Customer done mark ho gaya");
  };

  const markNoShow = (id) => {
    setQueue(prev => prev.map(e => e.id === id ? { ...e, status: "NoShow" } : e));
    showToast("No show mark ho gaya", "warn");
  };

  const activeQueue = queue.filter(e => ["Waiting", "Called", "Serving"].includes(e.status));
  const serving     = queue.find(e => e.status === "Serving" || e.status === "Called");
  const waiting     = queue.filter(e => e.status === "Waiting");

  const statusColor = (s) => ({
    Waiting: "gray", Called: "gold", Serving: "green", Done: "green", NoShow: "red"
  })[s] || "gray";

  return (
    <div style={{ minHeight: "100vh", background: "var(--cream)", display: "flex" }}>
      {/* Sidebar */}
      <div style={{ width: 240, background: "var(--ink)", display: "flex", flexDirection: "column", position: "fixed", top: 0, left: 0, bottom: 0 }}>
        <div style={{ padding: "28px 24px", borderBottom: "1px solid rgba(201,168,76,0.15)" }}>
          <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
            <div style={{ width: 32, height: 32, background: "var(--gold)", borderRadius: "50%", display: "flex", alignItems: "center", justifyContent: "center", fontSize: 16 }}>✂</div>
            <span style={{ fontFamily: "Playfair Display, serif", fontSize: 18, fontWeight: 700, color: "var(--cream)" }}>CutBook</span>
          </div>
          <div style={{ marginTop: 20, display: "flex", alignItems: "center", gap: 10 }}>
            <div style={{ width: 36, height: 36, background: "var(--gold)", borderRadius: "50%", display: "flex", alignItems: "center", justifyContent: "center", fontSize: 14, fontWeight: 700, color: "var(--ink)" }}>A</div>
            <div>
              <div style={{ fontSize: 13, fontWeight: 500, color: "var(--cream)" }}>Arjun's Salon</div>
              <div style={{ fontSize: 11, color: "#5a5248" }}>Pro Plan</div>
            </div>
          </div>
        </div>
        <nav style={{ flex: 1, padding: "16px 12px" }}>
          {[
            { id: "queue",    icon: "≡", label: "Live Queue" },
            { id: "analytics",icon: "↗", label: "Analytics"  },
            { id: "settings", icon: "⚙", label: "Settings"   },
          ].map(item => (
            <button key={item.id} onClick={() => setActiveTab(item.id)} style={{
              width: "100%", display: "flex", alignItems: "center", gap: 12,
              padding: "12px 16px", borderRadius: 8, border: "none", cursor: "pointer",
              background: activeTab === item.id ? "rgba(201,168,76,0.12)" : "transparent",
              color: activeTab === item.id ? "var(--gold)" : "#5a5248",
              fontFamily: "DM Sans, sans-serif", fontSize: 14, fontWeight: activeTab === item.id ? 500 : 400,
              marginBottom: 4, textAlign: "left", transition: "all 0.15s"
            }}>
              <span style={{ fontSize: 18 }}>{item.icon}</span>
              {item.label}
            </button>
          ))}
        </nav>
        <div style={{ padding: "16px 12px", borderTop: "1px solid rgba(201,168,76,0.1)" }}>
          <button onClick={() => onNavigate("home")} style={{
            width: "100%", padding: "10px", background: "transparent",
            border: "1px solid rgba(201,168,76,0.2)", borderRadius: 8, cursor: "pointer",
            color: "#5a5248", fontFamily: "DM Sans, sans-serif", fontSize: 13
          }}>← Logout</button>
        </div>
      </div>

      {/* Main content */}
      <div style={{ marginLeft: 240, flex: 1, padding: "32px 40px" }}>
        {/* Header */}
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 32 }}>
          <div>
            <h1 style={{ fontFamily: "Playfair Display, serif", fontSize: 32, fontWeight: 700 }}>Live Queue</h1>
            <p style={{ color: "var(--muted)", fontSize: 14, marginTop: 4 }}>Aaj — Sunday, 10 May · Real-time update</p>
          </div>
          <div style={{ display: "flex", gap: 12 }}>
            <button onClick={() => onNavigate("customer")} style={{
              padding: "12px 20px", background: "var(--white)", border: "1.5px solid #e0dbd2",
              borderRadius: 8, cursor: "pointer", fontFamily: "DM Sans, sans-serif",
              fontSize: 13, fontWeight: 500, color: "var(--ink)"
            }}>Customer View →</button>
            <button onClick={callNext} style={{
              padding: "12px 28px", background: "var(--gold)", border: "none",
              borderRadius: 8, cursor: "pointer", fontFamily: "DM Sans, sans-serif",
              fontSize: 14, fontWeight: 700, color: "var(--ink)",
              boxShadow: "0 4px 16px rgba(201,168,76,0.35)", transition: "all 0.2s",
              animation: "pulse-gold 2s infinite"
            }}>Next Customer →</button>
          </div>
        </div>

        {/* Stats cards */}
        <div style={{ display: "grid", gridTemplateColumns: "repeat(4, 1fr)", gap: 20, marginBottom: 32 }}>
          {[
            { label: "Wait mein",    value: waiting.length,                   icon: "⏳", color: "#fdf3d8", border: "#e8d5a3" },
            { label: "Aaj serve kiye",value: queue.filter(e=>e.status==="Done").length, icon: "✓",  color: "#e8f5ee", border: "#a8d5bb" },
            { label: "Avg. wait",    value: "12 min",                         icon: "⏱", color: "#faf7f2", border: "#e0dbd2" },
            { label: "Current token",value: serving ? `#${serving.tokenNumber}` : "—", icon: "✂",  color: "#fdf3d8", border: "#e8d5a3" },
          ].map(card => (
            <div key={card.label} style={{
              background: card.color, border: `1px solid ${card.border}`,
              borderRadius: 12, padding: "20px 24px"
            }}>
              <div style={{ fontSize: 22, marginBottom: 8 }}>{card.icon}</div>
              <div style={{ fontFamily: "Playfair Display, serif", fontSize: 28, fontWeight: 700 }}>{card.value}</div>
              <div style={{ fontSize: 12, color: "var(--muted)", marginTop: 4 }}>{card.label}</div>
            </div>
          ))}
        </div>

        {/* Currently serving */}
        {serving && (
          <div style={{
            background: "var(--ink)", borderRadius: 16, padding: "28px 32px",
            marginBottom: 24, display: "flex", justifyContent: "space-between", alignItems: "center",
            border: "1px solid rgba(201,168,76,0.2)"
          }}>
            <div style={{ display: "flex", alignItems: "center", gap: 20 }}>
              <div style={{
                width: 56, height: 56, background: "var(--gold)", borderRadius: "50%",
                display: "flex", alignItems: "center", justifyContent: "center",
                fontFamily: "Playfair Display, serif", fontSize: 22, fontWeight: 900, color: "var(--ink)"
              }}>#{serving.tokenNumber}</div>
              <div>
                <div style={{ fontSize: 11, color: "#5a5248", letterSpacing: "0.1em", marginBottom: 4 }}>ABHI SERVICE HO RAHI HAI</div>
                <div style={{ fontFamily: "Playfair Display, serif", fontSize: 22, fontWeight: 700, color: "var(--cream)" }}>{serving.customerName}</div>
                <div style={{ fontSize: 13, color: "#7a7268", marginTop: 2 }}>{serving.phone} · {serving.joinedAt}</div>
              </div>
            </div>
            <div style={{ display: "flex", gap: 12 }}>
              <button onClick={() => markDone(serving.id)} style={{
                padding: "10px 24px", background: "var(--green)", border: "none", borderRadius: 8,
                cursor: "pointer", fontFamily: "DM Sans, sans-serif", fontSize: 13, fontWeight: 600, color: "white"
              }}>Done ✓</button>
              <button onClick={() => markNoShow(serving.id)} style={{
                padding: "10px 20px", background: "transparent", border: "1px solid rgba(255,255,255,0.15)",
                borderRadius: 8, cursor: "pointer", fontFamily: "DM Sans, sans-serif", fontSize: 13, color: "#7a7268"
              }}>No Show</button>
            </div>
          </div>
        )}

        {/* Queue list */}
        <div style={{ background: "var(--white)", borderRadius: 16, border: "1px solid #e8e2d8", overflow: "hidden" }}>
          <div style={{ padding: "20px 28px", borderBottom: "1px solid #f0ece4", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
            <span style={{ fontFamily: "Playfair Display, serif", fontSize: 18, fontWeight: 600 }}>Waiting List</span>
            <Badge color="gold">{waiting.length} waiting</Badge>
          </div>
          {waiting.length === 0 && (
            <div style={{ padding: "60px 28px", textAlign: "center", color: "var(--muted)" }}>
              <div style={{ fontSize: 40, marginBottom: 12 }}>✂</div>
              <div style={{ fontFamily: "Playfair Display, serif", fontSize: 18 }}>Queue khali hai!</div>
              <div style={{ fontSize: 13, marginTop: 4 }}>Koi customer nahi wait kar raha</div>
            </div>
          )}
          {waiting.map((entry, i) => (
            <div key={entry.id} style={{
              padding: "18px 28px", borderBottom: "1px solid #f8f5f0",
              display: "flex", justifyContent: "space-between", alignItems: "center",
              animation: "fadeUp 0.3s ease", animationDelay: `${i * 0.05}s`, animationFillMode: "both",
              transition: "background 0.15s"
            }}
              onMouseEnter={e => e.currentTarget.style.background = "#faf7f2"}
              onMouseLeave={e => e.currentTarget.style.background = "transparent"}
            >
              <div style={{ display: "flex", alignItems: "center", gap: 16 }}>
                <div style={{
                  width: 40, height: 40, background: "#f0ece4", borderRadius: "50%",
                  display: "flex", alignItems: "center", justifyContent: "center",
                  fontFamily: "Playfair Display, serif", fontSize: 14, fontWeight: 700, color: "var(--gold-dark)"
                }}>#{entry.tokenNumber}</div>
                <div>
                  <div style={{ fontSize: 15, fontWeight: 500 }}>{entry.customerName}</div>
                  <div style={{ fontSize: 12, color: "var(--muted)", marginTop: 2 }}>{entry.phone} · Joined {entry.joinedAt}</div>
                </div>
              </div>
              <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
                <span style={{ fontSize: 12, color: "var(--muted)" }}>~{entry.wait} min</span>
                <Badge color={statusColor(entry.status)}>{entry.status}</Badge>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Toast */}
      {toast && (
        <div style={{
          position: "fixed", bottom: 28, right: 28, zIndex: 1000,
          background: "var(--ink)", color: "var(--cream)", padding: "14px 24px",
          borderRadius: 10, fontSize: 14, fontWeight: 500,
          borderLeft: `4px solid var(--gold)`, animation: "slideIn 0.3s ease",
          boxShadow: "0 8px 32px rgba(0,0,0,0.2)"
        }}>{toast.msg}</div>
      )}
    </div>
  );
}

// ─── SCREEN 4: Customer View ──────────────────────────────────────
function CustomerScreen({ onNavigate }) {
  const [stage, setStage] = useState("check"); // check | join | token
  const [form, setForm] = useState({ name: "", phone: "" });
  const [myToken, setMyToken] = useState(null);
  const [queueLen, setQueueLen] = useState(4);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (stage === "token") {
      const timer = setInterval(() => setQueueLen(n => Math.max(0, n - 1)), 8000);
      return () => clearInterval(timer);
    }
  }, [stage]);

  const joinQueue = () => {
    if (!form.name) return;
    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      setMyToken({ number: 52, position: 5, wait: 50 });
      setStage("token");
    }, 1000);
  };

  return (
    <div style={{ minHeight: "100vh", background: "var(--ink)", display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", padding: 24 }}>
      {/* Back */}
      <div style={{ position: "fixed", top: 24, left: 24 }}>
        <button onClick={() => onNavigate("home")} style={{
          background: "rgba(255,255,255,0.08)", border: "none", color: "var(--cream)",
          padding: "8px 16px", borderRadius: 20, cursor: "pointer", fontSize: 13
        }}>← Back</button>
      </div>

      <div style={{ width: "100%", maxWidth: 420, animation: "fadeUp 0.4s ease" }}>
        {/* Shop header */}
        <div style={{ textAlign: "center", marginBottom: 40 }}>
          <div style={{ width: 64, height: 64, background: "var(--gold)", borderRadius: "50%", display: "flex", alignItems: "center", justifyContent: "center", fontSize: 28, margin: "0 auto 16px" }}>✂</div>
          <h1 style={{ fontFamily: "Playfair Display, serif", fontSize: 28, fontWeight: 700, color: "var(--cream)" }}>Arjun's Salon</h1>
          <p style={{ color: "#7a7268", fontSize: 14, marginTop: 4 }}>Sector 17, Chandigarh</p>
          <div style={{ display: "inline-flex", alignItems: "center", gap: 6, marginTop: 12, background: "rgba(46,125,82,0.15)", border: "1px solid rgba(46,125,82,0.3)", padding: "4px 14px", borderRadius: 20 }}>
            <span style={{ width: 6, height: 6, background: "var(--green)", borderRadius: "50%" }} />
            <span style={{ fontSize: 12, color: "#4caf7d" }}>Open</span>
          </div>
        </div>

        {stage === "check" && (
          <div style={{ background: "rgba(255,255,255,0.04)", borderRadius: 20, padding: 32, border: "1px solid rgba(201,168,76,0.15)" }}>
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16, marginBottom: 28 }}>
              <div style={{ textAlign: "center", padding: "20px 12px", background: "rgba(255,255,255,0.04)", borderRadius: 12 }}>
                <div style={{ fontFamily: "Playfair Display, serif", fontSize: 36, fontWeight: 900, color: "var(--gold)" }}>4</div>
                <div style={{ fontSize: 12, color: "#5a5248", marginTop: 4 }}>Wait mein</div>
              </div>
              <div style={{ textAlign: "center", padding: "20px 12px", background: "rgba(255,255,255,0.04)", borderRadius: 12 }}>
                <div style={{ fontFamily: "Playfair Display, serif", fontSize: 36, fontWeight: 900, color: "var(--cream)" }}>~40</div>
                <div style={{ fontSize: 12, color: "#5a5248", marginTop: 4 }}>Min wait</div>
              </div>
            </div>
            <button onClick={() => setStage("join")} style={{
              width: "100%", padding: "16px", background: "var(--gold)", border: "none", borderRadius: 10,
              cursor: "pointer", fontFamily: "DM Sans, sans-serif", fontSize: 15, fontWeight: 700,
              color: "var(--ink)", letterSpacing: "0.02em"
            }}>Queue Mein Join Karo</button>
            <p style={{ textAlign: "center", fontSize: 12, color: "#5a5248", marginTop: 12 }}>
              Token lo, ghar se niklo sahi waqt pe
            </p>
          </div>
        )}

        {stage === "join" && (
          <div style={{ background: "rgba(255,255,255,0.04)", borderRadius: 20, padding: 32, border: "1px solid rgba(201,168,76,0.15)" }}>
            <h2 style={{ fontFamily: "Playfair Display, serif", fontSize: 22, fontWeight: 700, color: "var(--cream)", marginBottom: 24 }}>Apni detail do</h2>
            <div style={{ display: "flex", flexDirection: "column", gap: 16, marginBottom: 24 }}>
              <div>
                <label style={{ fontSize: 11, color: "#5a5248", letterSpacing: "0.08em", display: "block", marginBottom: 6 }}>NAAM *</label>
                <input value={form.name} onChange={e => setForm({...form, name: e.target.value})}
                  placeholder="Rahul Sharma"
                  style={{ width: "100%", padding: "14px 16px", background: "rgba(255,255,255,0.06)", border: "1.5px solid rgba(201,168,76,0.2)", borderRadius: 8, color: "var(--cream)", fontSize: 15, fontFamily: "DM Sans, sans-serif", outline: "none" }}
                  onFocus={e => e.target.style.borderColor = "var(--gold)"}
                  onBlur={e => e.target.style.borderColor = "rgba(201,168,76,0.2)"} />
              </div>
              <div>
                <label style={{ fontSize: 11, color: "#5a5248", letterSpacing: "0.08em", display: "block", marginBottom: 6 }}>PHONE (optional)</label>
                <input value={form.phone} onChange={e => setForm({...form, phone: e.target.value})}
                  placeholder="+91 98765 43210"
                  style={{ width: "100%", padding: "14px 16px", background: "rgba(255,255,255,0.06)", border: "1.5px solid rgba(201,168,76,0.2)", borderRadius: 8, color: "var(--cream)", fontSize: 15, fontFamily: "DM Sans, sans-serif", outline: "none" }}
                  onFocus={e => e.target.style.borderColor = "var(--gold)"}
                  onBlur={e => e.target.style.borderColor = "rgba(201,168,76,0.2)"} />
              </div>
            </div>
            <button onClick={joinQueue} disabled={!form.name || loading} style={{
              width: "100%", padding: "16px", background: form.name && !loading ? "var(--gold)" : "#4a4030",
              border: "none", borderRadius: 10, cursor: form.name ? "pointer" : "not-allowed",
              fontFamily: "DM Sans, sans-serif", fontSize: 15, fontWeight: 700,
              color: "var(--ink)", display: "flex", alignItems: "center", justifyContent: "center", gap: 8
            }}>
              {loading && <span style={{ width: 16, height: 16, border: "2px solid var(--ink)", borderTopColor: "transparent", borderRadius: "50%", animation: "spin 0.6s linear infinite" }} />}
              {loading ? "Join ho rahe ho..." : "Token Lo"}
            </button>
            <button onClick={() => setStage("check")} style={{ width: "100%", marginTop: 10, padding: "12px", background: "transparent", border: "none", cursor: "pointer", color: "#5a5248", fontFamily: "DM Sans, sans-serif", fontSize: 13 }}>← Wapas</button>
          </div>
        )}

        {stage === "token" && myToken && (
          <div style={{ textAlign: "center" }}>
            <div style={{
              background: "rgba(201,168,76,0.08)", border: "1px solid rgba(201,168,76,0.25)",
              borderRadius: 24, padding: "48px 32px", marginBottom: 20
            }}>
              <div style={{ fontSize: 12, color: "#5a5248", letterSpacing: "0.12em", marginBottom: 16 }}>TERA TOKEN NUMBER</div>
              <div style={{
                width: 120, height: 120, background: "var(--gold)", borderRadius: "50%",
                display: "flex", alignItems: "center", justifyContent: "center",
                fontFamily: "Playfair Display, serif", fontSize: 52, fontWeight: 900, color: "var(--ink)",
                margin: "0 auto 24px", boxShadow: "0 0 0 16px rgba(201,168,76,0.1), 0 0 0 32px rgba(201,168,76,0.05)"
              }}>#{myToken.number}</div>
              <div style={{ fontFamily: "Playfair Display, serif", fontSize: 28, fontWeight: 700, color: "var(--cream)", marginBottom: 8 }}>
                {form.name}
              </div>
              <div style={{ fontSize: 14, color: "#7a7268" }}>Arjun's Salon · Aaj</div>
            </div>

            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12, marginBottom: 20 }}>
              <div style={{ background: "rgba(255,255,255,0.04)", borderRadius: 12, padding: 20, border: "1px solid rgba(255,255,255,0.06)" }}>
                <div style={{ fontFamily: "Playfair Display, serif", fontSize: 28, fontWeight: 700, color: "var(--gold)" }}>{queueLen}</div>
                <div style={{ fontSize: 12, color: "#5a5248", marginTop: 4 }}>Tujhse aage</div>
              </div>
              <div style={{ background: "rgba(255,255,255,0.04)", borderRadius: 12, padding: 20, border: "1px solid rgba(255,255,255,0.06)" }}>
                <div style={{ fontFamily: "Playfair Display, serif", fontSize: 28, fontWeight: 700, color: "var(--cream)" }}>~{queueLen * 10}</div>
                <div style={{ fontSize: 12, color: "#5a5248", marginTop: 4 }}>Min mein tera number</div>
              </div>
            </div>

            <div style={{ background: "rgba(46,125,82,0.1)", border: "1px solid rgba(46,125,82,0.25)", borderRadius: 12, padding: "14px 20px", fontSize: 13, color: "#4caf7d", lineHeight: 1.6 }}>
              Ghar aaram se baith. Tera number aane pe WhatsApp aayega.
            </div>

            <button onClick={() => { setStage("check"); setForm({ name: "", phone: "" }); }} style={{
              marginTop: 20, width: "100%", padding: "14px", background: "transparent",
              border: "1px solid rgba(255,255,255,0.1)", borderRadius: 10, cursor: "pointer",
              color: "#5a5248", fontFamily: "DM Sans, sans-serif", fontSize: 13
            }}>← Naya Token</button>
          </div>
        )}
      </div>
    </div>
  );
}

// ─── ROOT APP ─────────────────────────────────────────────────────
export default function App() {
  const [screen, setScreen] = useState("home");

  return (
    <>
      <style>{theme}</style>
      {screen === "home"     && <LandingScreen   onNavigate={setScreen} />}
      {screen === "login"    && <AuthScreen       onNavigate={setScreen} mode="login"    />}
      {screen === "register" && <AuthScreen       onNavigate={setScreen} mode="register" />}
      {screen === "dashboard"&& <DashboardScreen  onNavigate={setScreen} />}
      {screen === "customer" && <CustomerScreen   onNavigate={setScreen} />}
    </>
  );
}