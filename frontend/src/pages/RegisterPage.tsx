import { useState } from "react";
import { useNavigate } from "react-router-dom";

function RegisterPage() {
    // state
    const [username, setUsername] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const navigate = useNavigate();

    // functions

    const handleRegister = async (e: React.FormEvent) => {
        // prevent page from reloading
        e.preventDefault(); 
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

            localStorage.setItem("token", data.jwtToken);
            alert("Registration successful");

            navigate("/login")

        } catch (error) {
            console.error(error);
            alert("Could not connect to the server");
        }
    };

    // UI

    return (
        <div className="container mt-5">
            <div className="row justify-content-center">
                <div className="col-md-4">

                    <div className="card shadow">
                        <div className="card-body">

                            <h2 className="text-center mb-4">
                                Register
                            </h2>

                            <form onSubmit={handleRegister}>
                                <div className="mb-3">
                                    <input
                                        className="form-control"
                                        value={username}
                                        onChange={(e) => setUsername(e.target.value)}
                                        type="text"
                                        placeholder="Username"
                                    />
                                </div>

                                <div className="mb-3">
                                    <input
                                        className="form-control"
                                        value={email}
                                        onChange={(e) => setEmail(e.target.value)}
                                        type="email"
                                        placeholder="Email"
                                    />
                                </div>

                                <div className="mb-3">
                                    <input
                                        className="form-control"
                                        value={password}
                                        onChange={(e) => setPassword(e.target.value)}
                                        type="password"
                                        placeholder="Password"
                                    />
                                </div>

                                <button
                                    className="btn btn-primary w-100"
                                    onClick={handleRegister}
                                >
                                    Register
                                </button>
                            </form>

                        </div>
                    </div>

                </div>
            </div>
        </div>
    );
}

export default RegisterPage;