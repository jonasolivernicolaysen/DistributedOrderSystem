import { useState } from "react";

function LoginPage() {

    // state
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    // functions
    const handleLogin = async () => {
        try {
            const response = await fetch(
                "https://localhost:7144/api/auth/login",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        username,
                        password
                    })
                });

            const data = await response.json();
            if (!response.ok) {
                alert(data.error);
                return;
            }
            alert("Login successful");
        } catch (error) {
            console.error(error);
            alert("Could not connect to the server");
        }
    };

    // UI
    return (
        <div>
            <h1>Login</h1>

            <input value={username} onChange={(e) => setUsername(e.target.value)} type="text" placeholder="Username" />

            <input value={password} onChange={(e) => setPassword(e.target.value)} type="password" placeholder="Password" />

            <button type="button" onClick={handleLogin}>Login</button>
        </div>
    );
}

export default LoginPage;