async function refreshToken() {
    const refreshToken = localStorage.getItem("refreshToken");

    const response = await fetch(
        "https://localhost:7144/api/auth/refresh",
        {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                refreshToken
            })
        });

    if (!response.ok)
        return false;

    const data = await response.json();

    localStorage.setItem("jwt", data.jwtToken);
    localStorage.setItem("refreshToken", data.refreshToken);

    return true;
}


export async function apiFetch(
    url: string,
    options: RequestInit = {}
) {
    const jwt = localStorage.getItem("jwt");

    let response = await fetch(url, {
        ...options,
        headers: {
            ...options.headers,
            Authorization: `Bearer ${jwt}`
        }
    });

    if (response.status === 401) {
        const refreshed = await refreshToken();

        if (!refreshed) {
            localStorage.removeItem("jwt");
            localStorage.removeItem("refreshToken");

            window.location.href = "/login";
            throw new Error("Unauthorized");
        }

        const newJwt = localStorage.getItem("jwt");

        response = await fetch(url, {
            ...options,
            headers: {
                ...options.headers,
                Authorization: `Bearer ${newJwt}`
            }
        });
    }

    return response;
}