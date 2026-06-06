import { useState } from "react";

function RegisterPage() {
    // state
    const [username, setUsername] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    // functions
    const handleRegister = async () => {
        try {
            const response = await fetch(
                "https://localhost:7144/api/auth/register",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        username,
                        email,
                        password
                    })
                });

            const data = await response.json();
            if (!response.ok) {
                alert(data.error);
                return;
            }
            alert("Registration successful");
        } catch (error) {
            console.error(error);
            alert("Could not connect to the server");
        }
    };

    // UI

    return (
        <div>
            <h1>Register</h1>

            <input value={username} onChange={(e) => setUsername(e.target.value)} type="text" placeholder="Username" />

            <input value={email} onChange={(e) => setEmail(e.target.value)} type="text" placeholder="Email" />

            <input value={password} onChange={(e) => setPassword(e.target.value)} type="password" placeholder="Password" />

            <button type="button" onClick={handleRegister}>Register</button>
        </div>
    );
}

export default RegisterPage;